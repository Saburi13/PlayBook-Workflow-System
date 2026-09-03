using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

using PlayBook.Business.DTOs.Approval;
using PlayBook.Business.DTOs.Workflow;
using PlayBook.Business.Services.Interfaces;
using PlayBook.Business.Services.Implementations;
using PlayBook.Data.Context;
using PlayBook.Domain;

namespace PlayBook.Infrastructure.Workflows;
 
public sealed class WorkflowExecutionService(
    PlayBookDbContext dbContext,
    IConditionEvaluator conditionEvaluator,
    IApprovalService approvalService,
    IPricingService? pricingService = null) : IWorkflowExecutionService
{
    private readonly IPricingService pricing = pricingService ?? new PricingCalculator();
    public async Task<IReadOnlyList<WorkflowExecutionDto>> TriggerAsync(string eventName, string entityType, Guid entityId, object? payload, CancellationToken cancellationToken = default)
    {
        var playBookIds = await dbContext.PlayBooks
            .Where(item => item.Status == PlayBookStatus.Active && item.TriggerType == TriggerType.Event)
            .Include(item => item.Steps)
            .Select(item => new
            {
                item.Id,
                Trigger = item.Steps.SingleOrDefault(step => step.IsStartStep && step.StepType == StepType.Trigger)!.ConfigurationJson
            })
            .ToListAsync(cancellationToken);

        var executions = new List<WorkflowExecutionDto>();
        foreach (var playBook in playBookIds)
        {
            if (!MatchesEvent(playBook.Trigger, eventName) || !await MatchesTriggerCriteriaAsync(playBook.Trigger, entityType, entityId, payload, cancellationToken)) continue;
            executions.Add(await StartAsync(new StartWorkflowRequest(playBook.Id, entityType, entityId, payload), cancellationToken));
        }

        return executions;
    }

    public async Task<WorkflowExecutionDto> StartAsync(StartWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var playBook = await LoadPlayBook(request.PlayBookId, cancellationToken);
        if (playBook is null) throw new KeyNotFoundException("PlayBook was not found.");
        if (playBook.Status != PlayBookStatus.Active) throw new InvalidOperationException("Only active PlayBooks can be started.");

        var startStep = playBook.Steps.SingleOrDefault(step => step.IsStartStep);
        if (startStep is null) throw new InvalidOperationException("The PlayBook must have exactly one start step.");
        if (playBook.Steps.Count(step => step.IsStartStep) > 1) throw new InvalidOperationException("The PlayBook must have exactly one start step.");

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            PlayBookId = playBook.Id,
            PlayBook = playBook,
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            CurrentStepId = startStep.Id,
            Status = WorkflowStatus.Running
        };
        dbContext.WorkflowExecutions.Add(execution);
        await ProcessAsync(execution, playBook, startStep, request.Payload, request.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase) ? await LoadProposal(request.EntityId, cancellationToken) : null, false, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(execution);
    }

    public async Task<WorkflowExecutionDto> ResumeAsync(Guid executionId, object? payload, CancellationToken cancellationToken = default)
    {
        var execution = await dbContext.WorkflowExecutions
            .Include(item => item.PlayBook).ThenInclude(playBook => playBook.Steps)
            .Include(item => item.PlayBook).ThenInclude(playBook => playBook.Transitions).ThenInclude(transition => transition.Condition)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == executionId, cancellationToken);

        if (execution is null) throw new KeyNotFoundException("Workflow execution was not found.");
        if (execution.Status != WorkflowStatus.Waiting) throw new InvalidOperationException("Only waiting executions can be resumed.");
        var currentStep = execution.PlayBook.Steps.SingleOrDefault(step => step.Id == execution.CurrentStepId);
        if (currentStep is null) throw new InvalidOperationException("The current workflow step no longer exists.");

        object? workflowModel = null;
        if (execution.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase))
        {
            var proposal = await dbContext.Proposals
                .Include(item => item.Opportunity)
                .Include(item => item.ProposalProducts)
                    .ThenInclude(product => product.Product)
                .SingleOrDefaultAsync(item => item.Id == execution.EntityId, cancellationToken);
            if (proposal is null) throw new KeyNotFoundException("Proposal was not found for workflow resume.");
            workflowModel = proposal;

            if (currentStep.StepType == StepType.Approval)
            {
                var approvalDecision = ResolveApprovalOutcome(payload, null);
                var approval = await dbContext.Approvals
                    .SingleOrDefaultAsync(item => item.WorkflowExecutionId == execution.Id && item.ProposalId == execution.EntityId && item.Status == ApprovalStatus.Pending, cancellationToken);

                if (approval is not null)
                {
                    approval.Status = approvalDecision;
                    approval.RespondedAt = DateTime.UtcNow;
                    approval.UpdatedAt = DateTime.UtcNow;
                    approval.Comments ??= approvalDecision == ApprovalStatus.Approved ? "Approved via workflow resume." : "Rejected via workflow resume.";
                }

                proposal.Status = approvalDecision == ApprovalStatus.Approved ? ProposalStatus.Approved : ProposalStatus.Rejected;
                proposal.UpdatedAt = DateTime.UtcNow;
            }
            else if (currentStep.StepType == StepType.CustomerAction)
            {
                var customerDecision = ResolveApprovalOutcome(payload, null);
                proposal.Status = customerDecision == ApprovalStatus.Approved ? ProposalStatus.CustomerApproved : ProposalStatus.CustomerRejected;
                proposal.UpdatedAt = DateTime.UtcNow;

                if (customerDecision == ApprovalStatus.Approved)
                {
                    var calculatedPricing = proposal.ProposalProducts.Count == 0 ? null : await CalculateProposalPricingAsync(proposal, cancellationToken);
                    var authoritativeTotal = calculatedPricing?.TotalAmount ?? proposal.TotalAmount;
                    if (calculatedPricing is not null)
                    {
                        proposal.SubTotal = calculatedPricing.Subtotal;
                        proposal.DiscountAmount = calculatedPricing.LineDiscountAmount;
                        proposal.VoucherDiscountAmount = calculatedPricing.VoucherDiscountAmount;
                        proposal.TotalAmount = authoritativeTotal;
                    }
                    var existingOrder = await dbContext.Orders
                        .SingleOrDefaultAsync(item => item.ProposalId == proposal.Id, cancellationToken);
                    existingOrder ??= dbContext.ChangeTracker.Entries<Order>()
                        .Select(entry => entry.Entity)
                        .SingleOrDefault(item => item.ProposalId == proposal.Id && dbContext.Entry(item).State != EntityState.Deleted);

                    if (existingOrder is null)
                    {
                        var order = new Order
                        {
                            Id = Guid.NewGuid(),
                            ProposalId = proposal.Id,
                            CustomerId = proposal.CustomerId,
                            AssignedEmployeeId = proposal.Opportunity.AssignedEmployeeId ?? proposal.CreatedByEmployeeId,
                            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                            Status = OrderStatus.Pending,
                            TotalAmount = authoritativeTotal,
                            OrderDate = DateTime.UtcNow
                        };
                        if (calculatedPricing is not null)
                        {
                            order.OrderProducts = calculatedPricing.Lines!.Select(line => new OrderProduct { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, Discount = line.DiscountAmount, TotalPrice = line.TotalAmount }).ToList();
                        }
                        dbContext.Orders.Add(order);
                    }

                    foreach (var item in proposal.ProposalProducts)
                    {
                        var product = item.Product;
                        if (product is null)
                        {
                            continue;
                        }

                        var existingSubscription = await dbContext.Subscriptions
                            .OrderByDescending(subscription => subscription.StartDate)
                            .FirstOrDefaultAsync(subscription =>
                                subscription.CustomerId == proposal.CustomerId &&
                                subscription.ProductId == product.Id &&
                                subscription.Status != SubscriptionStatus.Cancelled,
                                cancellationToken);
                            existingSubscription ??= dbContext.ChangeTracker.Entries<Subscription>()
                                .Select(entry => entry.Entity)
                                .OrderByDescending(subscription => subscription.StartDate)
                                .FirstOrDefault(subscription =>
                                subscription.CustomerId == proposal.CustomerId &&
                                subscription.ProductId == product.Id &&
                                subscription.Status != SubscriptionStatus.Cancelled &&
                                dbContext.Entry(subscription).State != EntityState.Deleted);

                            if (existingSubscription is not null && (existingSubscription.Status is SubscriptionStatus.Expiring or SubscriptionStatus.Expired || existingSubscription.EndDate <= DateTime.UtcNow))
                        {
                            var renewedSubscription = RenewExpiringSubscription(existingSubscription, DateTime.UtcNow, product.PlanDurationMonths);
                            if (renewedSubscription is null) continue;
                            renewedSubscription.CustomerId = proposal.CustomerId;
                            renewedSubscription.ProductId = product.Id;
                            renewedSubscription.Amount = item.TotalPrice;
                            dbContext.Subscriptions.Add(renewedSubscription);
                            continue;
                        }

                        if (existingSubscription is not null) continue;

                        dbContext.Subscriptions.Add(new Subscription
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = proposal.CustomerId,
                            ProductId = product.Id,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddMonths(product.PlanDurationMonths ?? 12),
                            Amount = item.TotalPrice,
                            Status = SubscriptionStatus.Active
                        });
                    }

                    if (proposal.Opportunity is not null)
                    {
                        proposal.Opportunity.Status = OpportunityStatus.Won;
                        proposal.Opportunity.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else if (proposal.Opportunity is not null)
                {
                    proposal.Opportunity.Status = OpportunityStatus.Proposal;
                    proposal.Opportunity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        execution.Status = WorkflowStatus.Running;
        await ProcessAsync(execution, execution.PlayBook, currentStep, payload, workflowModel, true, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(execution);
    }

    private async Task ProcessAsync(WorkflowExecution execution, PlayBook.Domain.PlayBook playBook, PlayBookStep? currentStep, object? payload, object? workflowModel, bool skipBlockingStep, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        var shouldProcessStep = !skipBlockingStep;

        while (currentStep is not null)
        {
            if (!visited.Add(currentStep.Id))
            {
                Fail(execution, "Workflow contains a cycle without a waiting or end step.");
                return;
            }

            if (shouldProcessStep)
            {
                var executionStep = new WorkflowExecutionStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowExecutionId = execution.Id,
                    PlayBookStepId = currentStep.Id,
                    StartedAt = DateTime.UtcNow,
                    Status = WorkflowStatus.Running
                };
                execution.Steps.Add(executionStep);
                dbContext.WorkflowExecutionSteps.Add(executionStep);
                AddHistory(execution, currentStep, "StepStarted");

                if (currentStep.IsEndStep || currentStep.StepType == StepType.End)
                {
                    CompleteStep(executionStep, "End");
                    Complete(execution);
                    return;
                }

                if (currentStep.StepType is StepType.Action or StepType.Notification or StepType.EmployeeAssignment)
                {
                    workflowModel = await ExecuteActionAsync(execution, currentStep, payload, cancellationToken);
                }

                if (currentStep.StepType is StepType.Approval or StepType.Wait or StepType.CustomerAction)
                {
                    if (currentStep.StepType == StepType.Approval && string.Equals(execution.EntityType, "Proposal", StringComparison.OrdinalIgnoreCase))
                    {
                        await approvalService.RequestAsync(execution.EntityId, execution.Id, cancellationToken);
                    }

                    executionStep.Status = WorkflowStatus.Waiting;
                    executionStep.CompletedAt = DateTime.UtcNow;
                    execution.CurrentStepId = currentStep.Id;
                    execution.Status = WorkflowStatus.Waiting;
                    AddHistory(execution, currentStep, "Waiting");
                    return;
                }

                CompleteStep(executionStep, "Completed");
                AddHistory(execution, currentStep, "StepCompleted");
            }

            var transition = playBook.Transitions
                .Where(item => item.FromStepId == currentStep.Id)
                .OrderBy(item => item.Priority)
                .FirstOrDefault(item => IsForcedManagerApproval(payload, currentStep, item) || IsTransitionValid(item, workflowModel ?? payload));

            if (transition is null)
            {
                Fail(execution, $"No valid transition exists from step '{currentStep.Name}'.");
                return;
            }

            currentStep = playBook.Steps.SingleOrDefault(step => step.Id == transition.ToStepId);
            if (currentStep is null)
            {
                Fail(execution, "A workflow transition points to a missing step.");
                return;
            }

            execution.CurrentStepId = currentStep.Id;
            shouldProcessStep = true;
            await Task.CompletedTask;
        }
    }

    private async Task<PlayBook.Domain.PlayBook?> LoadPlayBook(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PlayBooks
            .Include(playBook => playBook.Steps).ThenInclude(step => step.Conditions)
            .Include(playBook => playBook.Transitions).ThenInclude(transition => transition.Condition)
            .AsSplitQuery()
            .SingleOrDefaultAsync(playBook => playBook.Id == id, cancellationToken);

    private bool IsTransitionValid(WorkflowTransition transition, object? model)
    {
        if (transition.Condition is null) return true;
        var result = conditionEvaluator.Evaluate(transition.Condition.Field, transition.Condition.Operator.ToString(), transition.Condition.Value, model, transition.Condition.DataType);
        return string.Equals(transition.Label, "FALSE", StringComparison.OrdinalIgnoreCase) ? !result : result;
    }

    private static bool IsForcedManagerApproval(object? payload, PlayBookStep step, WorkflowTransition transition) =>
        step.Name.Equals("Check Discount", StringComparison.OrdinalIgnoreCase) &&
        transition.Label.Equals("TRUE", StringComparison.OrdinalIgnoreCase) &&
        payload is not null &&
        payload.GetType().GetProperty("forceManagerApproval")?.GetValue(payload) is true;

    private async Task<Proposal?> LoadProposal(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Proposals.Include(item => item.Opportunity).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    private async Task<object?> ExecuteActionAsync(WorkflowExecution execution, PlayBookStep step, object? payload, CancellationToken cancellationToken)
    {
        using var document = string.IsNullOrWhiteSpace(step.ConfigurationJson) ? null : JsonDocument.Parse(step.ConfigurationJson);
        var actionType = document?.RootElement.TryGetProperty("actionType", out var actionElement) == true ? actionElement.GetString() : step.Name;

        if (string.Equals(actionType, "Create Proposal", StringComparison.OrdinalIgnoreCase) && execution.EntityType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase))
        {
            var opportunity = await dbContext.Opportunities.SingleOrDefaultAsync(item => item.Id == execution.EntityId, cancellationToken);
            if (opportunity is null) throw new KeyNotFoundException("Opportunity was not found for workflow action.");
            var employeeId = opportunity.AssignedEmployeeId ?? await dbContext.Employees.Select(item => item.Id).FirstOrDefaultAsync(cancellationToken);
            if (employeeId == Guid.Empty) throw new InvalidOperationException("An employee is required to create a proposal.");
            var discount = ReadDecimal(payload, "discountPercentage", 0m);
            var total = opportunity.EstimatedValue;
            var proposal = new Proposal { Id = Guid.NewGuid(), OpportunityId = opportunity.Id, CustomerId = opportunity.CustomerId, CreatedByEmployeeId = employeeId, ProposalNumber = $"P-{DateTime.UtcNow:yyyyMMddHHmmss}", Status = ProposalStatus.Draft, SubTotal = total, DiscountPercentage = discount, DiscountAmount = total * discount / 100m, TotalAmount = total - total * discount / 100m, ValidUntil = DateTime.UtcNow.AddDays(30) };
            var configuredLines = await ReadProductLinesAsync(payload, cancellationToken);
            if (configuredLines.Count > 0)
            {
                proposal.SubTotal = configuredLines.Sum(line => line.Subtotal);
                proposal.DiscountAmount = configuredLines.Sum(line => line.DiscountAmount);
                proposal.TotalAmount = proposal.SubTotal - proposal.DiscountAmount;
                proposal.DiscountPercentage = proposal.SubTotal == 0 ? 0 : proposal.DiscountAmount / proposal.SubTotal * 100m;
                foreach (var line in configuredLines)
                {
                    proposal.ProposalProducts.Add(new ProposalProduct { Id = Guid.NewGuid(), ProposalId = proposal.Id, ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, DiscountPercentage = line.DiscountPercentage, DiscountAmount = line.DiscountAmount, TotalPrice = line.TotalPrice });
                }
            }
            else
            {
                var product = await dbContext.Products.Where(item => item.IsActive).OrderBy(item => item.Name).FirstOrDefaultAsync(cancellationToken);
                if (product is not null) proposal.ProposalProducts.Add(new ProposalProduct { Id = Guid.NewGuid(), ProposalId = proposal.Id, ProductId = product.Id, Quantity = 1, UnitPrice = proposal.TotalAmount, TotalPrice = proposal.TotalAmount });
            }
            dbContext.Proposals.Add(proposal);
            await dbContext.SaveChangesAsync(cancellationToken);
            execution.EntityType = "Proposal";
            execution.EntityId = proposal.Id;
            return proposal;
        }

        if (string.Equals(actionType, "Auto Approval", StringComparison.OrdinalIgnoreCase) && execution.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase))
        {
            var proposal = await LoadProposal(execution.EntityId, cancellationToken);
            if (proposal is null) throw new KeyNotFoundException("Proposal was not found for automatic approval.");
            proposal.Status = ProposalStatus.Approved;
            proposal.UpdatedAt = DateTime.UtcNow;
            return proposal;
        }

        if (string.Equals(actionType, "Create Order", StringComparison.OrdinalIgnoreCase) && execution.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase))
        {
            var proposal = await dbContext.Proposals.Include(item => item.ProposalProducts).SingleOrDefaultAsync(item => item.Id == execution.EntityId, cancellationToken);
            if (proposal is null) throw new KeyNotFoundException("Proposal was not found for order creation.");
            var order = await dbContext.Orders.Include(item => item.OrderProducts).SingleOrDefaultAsync(item => item.ProposalId == proposal.Id, cancellationToken);
            order ??= dbContext.ChangeTracker.Entries<Order>()
                .Select(entry => entry.Entity)
                .SingleOrDefault(item => item.ProposalId == proposal.Id && dbContext.Entry(item).State != EntityState.Deleted);
            if (order is not null) return proposal;
            order = new Order { Id = Guid.NewGuid(), ProposalId = proposal.Id, CustomerId = proposal.CustomerId, OrderNumber = $"ORD-{Guid.NewGuid():N}"[..12], TotalAmount = proposal.TotalAmount, Status = OrderStatus.Pending };
            order.OrderProducts = proposal.ProposalProducts.Select(line => new OrderProduct { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, Discount = line.DiscountAmount, TotalPrice = line.TotalPrice }).ToList();
            dbContext.Orders.Add(order);
            return proposal;
        }

        if (string.Equals(actionType, "Update Status", StringComparison.OrdinalIgnoreCase))
        {
            var status = ReadString(step.ConfigurationJson, "status");
            if (string.IsNullOrWhiteSpace(status)) return null;
            if (execution.EntityType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase) && Enum.TryParse<OpportunityStatus>(status, true, out var opportunityStatus))
            {
                var opportunity = await dbContext.Opportunities.SingleOrDefaultAsync(item => item.Id == execution.EntityId, cancellationToken);
                if (opportunity is not null) opportunity.Status = opportunityStatus;
            }
            else if (execution.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase) && Enum.TryParse<ProposalStatus>(status, true, out var proposalStatus))
            {
                var proposal = await dbContext.Proposals.SingleOrDefaultAsync(item => item.Id == execution.EntityId, cancellationToken);
                if (proposal is not null) proposal.Status = proposalStatus;
            }
            return null;
        }

        if (string.Equals(actionType, "Assign Employee", StringComparison.OrdinalIgnoreCase) || string.Equals(actionType, "Employee Assignment", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = ReadGuid(step.ConfigurationJson, "employeeId") ?? ReadGuid(payload, "employeeId");
            if (employeeId is null || !await dbContext.Employees.AnyAsync(item => item.Id == employeeId && item.IsActive, cancellationToken)) return null;
            if (execution.EntityType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase))
            {
                var opportunity = await dbContext.Opportunities.SingleOrDefaultAsync(item => item.Id == execution.EntityId, cancellationToken);
                if (opportunity is not null) opportunity.AssignedEmployeeId = employeeId;
            }
            return null;
        }

        if (string.Equals(actionType, "Create Activity", StringComparison.OrdinalIgnoreCase) || string.Equals(actionType, "Send Notification", StringComparison.OrdinalIgnoreCase) || string.Equals(actionType, "Notification", StringComparison.OrdinalIgnoreCase))
        {
            var customerId = await ResolveCustomerIdAsync(execution, cancellationToken);
            if (customerId is not null && string.Equals(actionType, "Create Activity", StringComparison.OrdinalIgnoreCase))
            {
                dbContext.EngagementActivities.Add(new EngagementActivity { Id = Guid.NewGuid(), CustomerId = customerId.Value, OpportunityId = execution.EntityType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase) ? execution.EntityId : null, ProposalId = execution.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase) ? execution.EntityId : null, Type = ReadString(step.ConfigurationJson, "activityType") ?? "Follow-up", Subject = ReadString(step.ConfigurationJson, "subject") ?? step.Name, Description = ReadString(step.ConfigurationJson, "description") });
            }
            AddHistory(execution, step, actionType ?? step.Name);
            return null;
        }

        return null;
    }

    private async Task<bool> MatchesTriggerCriteriaAsync(string? configurationJson, string entityType, Guid entityId, object? payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configurationJson)) return false;
        using var document = JsonDocument.Parse(configurationJson);
        var root = document.RootElement;
        var employeeId = ReadGuid(root, "employeeId") ?? ReadGuid(root, "assignedEmployeeId");
        var role = root.TryGetProperty("role", out var roleElement) ? roleElement.GetString() : null;
        var gradeId = ReadGuid(root, "employeeGradeId");
        var approvalLevel = root.TryGetProperty("approvalLevel", out var levelElement) && levelElement.TryGetInt32(out var level) ? level : (int?)null;
        if (employeeId is null && role is null && gradeId is null && approvalLevel is null) return true;
        var contextEmployeeId = ReadGuid(payload, "employeeId") ?? ReadGuid(payload, "assignedEmployeeId");
        if (contextEmployeeId is null && entityType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase)) contextEmployeeId = await dbContext.Opportunities.Where(item => item.Id == entityId).Select(item => item.AssignedEmployeeId).SingleOrDefaultAsync(cancellationToken);
        if (contextEmployeeId is null && entityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase)) contextEmployeeId = await dbContext.Proposals.Where(item => item.Id == entityId).Select(item => (Guid?)item.CreatedByEmployeeId).SingleOrDefaultAsync(cancellationToken);
        if (contextEmployeeId is null) return false;
        var employee = await dbContext.Employees.Include(item => item.EmployeeGrade).SingleOrDefaultAsync(item => item.Id == contextEmployeeId, cancellationToken);
        return employee is not null && (employeeId is null || employee.Id == employeeId) && (role is null || employee.Role.ToString().Equals(role, StringComparison.OrdinalIgnoreCase)) && (gradeId is null || employee.EmployeeGradeId == gradeId) && (approvalLevel is null || employee.EmployeeGrade?.ApprovalLimit >= approvalLevel);
    }

    private static decimal ReadDecimal(object? payload, string propertyName, decimal fallback)
    {
        if (payload is JsonElement element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number)) return number;
            if (decimal.TryParse(property.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var text)) return text;
        }
        return fallback;
    }

    private static string? ReadString(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }

    private static Guid? ReadGuid(object? payload, string propertyName)
    {
        if (payload is JsonElement element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) && Guid.TryParse(property.GetString(), out var id)) return id;
        return null;
    }

    private static Guid? ReadGuid(JsonElement element, string propertyName) => element.TryGetProperty(propertyName, out var property) && Guid.TryParse(property.GetString(), out var id) ? id : null;

    private async Task<Guid?> ResolveCustomerIdAsync(WorkflowExecution execution, CancellationToken cancellationToken) =>
        execution.EntityType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase)
            ? await dbContext.Opportunities.Where(item => item.Id == execution.EntityId).Select(item => (Guid?)item.CustomerId).SingleOrDefaultAsync(cancellationToken)
            : execution.EntityType.Equals("Proposal", StringComparison.OrdinalIgnoreCase)
                ? await dbContext.Proposals.Where(item => item.Id == execution.EntityId).Select(item => (Guid?)item.CustomerId).SingleOrDefaultAsync(cancellationToken)
                : execution.EntityType.Equals("Subscription", StringComparison.OrdinalIgnoreCase)
                    ? await dbContext.Subscriptions.Where(item => item.Id == execution.EntityId).Select(item => (Guid?)item.CustomerId).SingleOrDefaultAsync(cancellationToken)
                    : null;

    private async Task<PricingResult> CalculateProposalPricingAsync(Proposal proposal, CancellationToken cancellationToken)
    {
        var lines = proposal.ProposalProducts.Select(item => new PricingLine(item.ProductId, item.Quantity, item.Product?.Price ?? item.UnitPrice, item.DiscountType, item.DiscountValue == 0m ? item.DiscountPercentage : item.DiscountValue));
        var vouchers = new List<PricingVoucher>();
        if (!string.IsNullOrWhiteSpace(proposal.VoucherCode))
        {
            var voucher = await dbContext.Vouchers.SingleOrDefaultAsync(item => item.Code == proposal.VoucherCode, cancellationToken);
            var validation = new VoucherService().Validate(voucher, pricing.Calculate(lines).TotalAmount, DateTime.UtcNow);
            if (!validation.IsValid) throw new InvalidOperationException(validation.Error);
            vouchers.Add(validation.Voucher!);
        }
        return pricing.Calculate(lines, vouchers);
    }

    private async Task<IReadOnlyList<ConfiguredProductLine>> ReadProductLinesAsync(object? payload, CancellationToken cancellationToken)
    {
        if (payload is not JsonElement element || element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("products", out var products) || products.ValueKind != JsonValueKind.Array)
            return [];

        var requested = products.EnumerateArray()
            .Select(item => new { Id = item.TryGetProperty("productId", out var id) && Guid.TryParse(id.GetString(), out var productId) ? productId : Guid.Empty, Quantity = item.TryGetProperty("quantity", out var quantity) && quantity.TryGetInt32(out var count) ? count : 1, UnitPrice = item.TryGetProperty("unitPrice", out var unitPrice) && unitPrice.TryGetDecimal(out var price) ? price : (decimal?)null, Discount = item.TryGetProperty("discountPercentage", out var lineDiscount) && lineDiscount.TryGetDecimal(out var discountPercentage) ? discountPercentage : 0m })
            .Where(item => item.Id != Guid.Empty && item.Quantity > 0)
            .ToList();
        if (requested.Count == 0) return [];

        var productIds = requested.Select(item => item.Id).Distinct().ToList();
        var productsById = await dbContext.Products.Where(product => product.IsActive && productIds.Contains(product.Id)).ToDictionaryAsync(product => product.Id, cancellationToken);
        return requested.Where(item => productsById.ContainsKey(item.Id)).Select(item =>
        {
            var unitPrice = item.UnitPrice ?? productsById[item.Id].Price;
            var subtotal = unitPrice * item.Quantity;
            var discountAmount = subtotal * item.Discount / 100m;
            return new ConfiguredProductLine(item.Id, item.Quantity, unitPrice, item.Discount, subtotal, discountAmount, subtotal - discountAmount);
        }).ToList();
    }

    private sealed record ConfiguredProductLine(Guid ProductId, int Quantity, decimal UnitPrice, decimal DiscountPercentage, decimal Subtotal, decimal DiscountAmount, decimal TotalPrice);

    private static bool MatchesEvent(string? configurationJson, string eventName)
    {
        if (string.IsNullOrWhiteSpace(configurationJson)) return false;

        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            return document.RootElement.TryGetProperty("event", out var eventElement) &&
                   string.Equals(eventElement.GetString(), eventName, StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private void AddHistory(WorkflowExecution execution, PlayBookStep step, string action)
    {
        var history = new WorkflowHistory { Id = Guid.NewGuid(), WorkflowExecutionId = execution.Id, StepId = step.Id, Action = action, Timestamp = DateTime.UtcNow };
        execution.Histories.Add(history);
        dbContext.WorkflowHistories.Add(history);
    }

    private static void CompleteStep(WorkflowExecutionStep step, string result)
    {
        step.Status = WorkflowStatus.Completed;
        step.Result = result;
        step.CompletedAt = DateTime.UtcNow;
    }

    private static void Complete(WorkflowExecution execution)
    {
        execution.Status = WorkflowStatus.Completed;
        execution.CompletedAt = DateTime.UtcNow;
        execution.CurrentStepId = null;
    }

    private static void Fail(WorkflowExecution execution, string message)
    {
        execution.Status = WorkflowStatus.Failed;
        execution.ErrorMessage = message;
        execution.CompletedAt = DateTime.UtcNow;
    }

    public static Subscription? RenewExpiringSubscription(Subscription subscription, DateTime? currentTime = null, int? durationMonths = null)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        var now = currentTime ?? DateTime.UtcNow;
        if (subscription.Status == SubscriptionStatus.Cancelled || subscription.Status == SubscriptionStatus.Renewed)
        {
            return null;
        }

        subscription.Status = SubscriptionStatus.Renewed;
        subscription.UpdatedAt = now;

        return new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = subscription.CustomerId,
            ProductId = subscription.ProductId,
            StartDate = now,
            EndDate = now.AddMonths(durationMonths ?? 12),
            Amount = subscription.Amount,
            Status = SubscriptionStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static void UpdateSubscriptionStatus(Subscription subscription, DateTime? currentTime = null)
    {
        var now = currentTime ?? DateTime.UtcNow;
        if (subscription.Status == SubscriptionStatus.Cancelled || subscription.Status == SubscriptionStatus.Expired || subscription.Status == SubscriptionStatus.Renewed)
        {
            return;
        }

        if (subscription.EndDate <= now)
        {
            subscription.Status = SubscriptionStatus.Expired;
            subscription.UpdatedAt = now;
            return;
        }

        if (subscription.EndDate <= now.AddDays(30))
        {
            subscription.Status = SubscriptionStatus.Expiring;
            subscription.UpdatedAt = now;
        }
    }

    public static ApprovalStatus ResolveApprovalOutcome(object? payload, ApprovalStatus? fallback)
    {
        if (payload is ApprovalDecisionRequestDto request)
        {
            return request.Decision is ApprovalStatus.Approved or ApprovalStatus.Rejected ? request.Decision : fallback ?? ApprovalStatus.Pending;
        }

        if (payload is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("decision", out var decisionElement))
                {
                    var parsedDecision = ParseApprovalStatus(decisionElement);
                    if (parsedDecision.HasValue) return parsedDecision.Value;
                }

                if (element.TryGetProperty("status", out var statusElement))
                {
                    var parsedStatus = ParseApprovalStatus(statusElement);
                    if (parsedStatus.HasValue) return parsedStatus.Value;
                }
            }
        }

        if (payload is not null)
        {
            var type = payload.GetType();

            var decision = type.GetProperty("Decision")?.GetValue(payload) ??
                           type.GetProperty("decision")?.GetValue(payload) ??
                           type.GetProperty("Status")?.GetValue(payload) ??
                           type.GetProperty("status")?.GetValue(payload);

            if (decision is ApprovalStatus approvalStatus)
            {
                return approvalStatus;
            }

            if (decision is string decisionText && Enum.TryParse<ApprovalStatus>(decisionText, true, out var parsedStatus))
            {
                return parsedStatus;
            }
        }

        return fallback ?? ApprovalStatus.Pending;
    }

    private static ApprovalStatus? ParseApprovalStatus(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var text = element.GetString();
            if (Enum.TryParse<ApprovalStatus>(text, true, out var parsedStatus))
            {
                return parsedStatus;
            }
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var numericValue) && Enum.IsDefined(typeof(ApprovalStatus), numericValue))
        {
            return (ApprovalStatus)numericValue;
        }

        return null;
    }

    private static WorkflowExecutionDto ToDto(WorkflowExecution execution) =>
        new(execution.Id, execution.PlayBookId, execution.EntityType, execution.EntityId, execution.CurrentStepId, execution.Status, execution.ErrorMessage);
}
