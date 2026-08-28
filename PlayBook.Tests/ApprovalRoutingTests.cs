using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlayBook.Application.Approvals;
using PlayBook.Application.Services;
using PlayBook.Application.Workflows;
using PlayBook.Domain;
using PlayBook.Infrastructure.Approvals;
using PlayBook.Infrastructure.Data;
using PlayBook.Infrastructure.Workflows;

namespace PlayBook.Tests;

public sealed class ApprovalRoutingTests
{
    [Fact]
    public void ApprovalDecisionRequestAcceptsOnlyDecisionStatesAtServiceBoundary()
    {
        var request = new ApprovalDecisionRequest(Guid.NewGuid(), ApprovalStatus.Approved, "Reviewed");

        Assert.Equal(ApprovalStatus.Approved, request.Decision);
        Assert.Equal("Reviewed", request.Comments);
    }

    [Fact]
    public void SeededHierarchyHasManagerChainForApprovalRouting()
    {
        var anand = new Employee { Id = Guid.NewGuid(), FirstName = "Anand" };
        var abdul = new Employee { Id = Guid.NewGuid(), FirstName = "Abdul", Manager = anand, ManagerId = anand.Id };
        var aditya = new Employee { Id = Guid.NewGuid(), FirstName = "Aditya", Manager = abdul, ManagerId = abdul.Id };

        Assert.Equal(abdul.Id, aditya.ManagerId);
        Assert.Equal(anand.Id, abdul.ManagerId);
    }

    [Fact]
    public void ApprovalDecisionRequest_TracksApprovalOutcome()
    {
        var request = new ApprovalDecisionRequest(Guid.NewGuid(), ApprovalStatus.Rejected, "Needs correction");

        Assert.Equal(ApprovalStatus.Rejected, request.Decision);
        Assert.Equal("Needs correction", request.Comments);
    }

    [Fact]
    public void WorkflowResumeDecisionsCanBeResolvedFromJsonPayload()
    {
        using var document = JsonDocument.Parse("""{"decision":"Approved","comments":"Looks good"}""");

        var outcome = WorkflowExecutionService.ResolveApprovalOutcome(document.RootElement, null);

        Assert.Equal(ApprovalStatus.Approved, outcome);
    }

    [Fact]
    public void BuildApprovalHierarchy_ReturnsManagersInApprovalOrder()
    {
        var anand = new Employee { Id = Guid.NewGuid(), FirstName = "Anand" };
        var abdul = new Employee { Id = Guid.NewGuid(), FirstName = "Abdul", Manager = anand, ManagerId = anand.Id };
        var aditya = new Employee { Id = Guid.NewGuid(), FirstName = "Aditya", Manager = abdul, ManagerId = abdul.Id };

        var hierarchy = ApprovalService.BuildApprovalHierarchy(aditya);

        Assert.Equal(new[] { "Anand", "Abdul", "Aditya" }, hierarchy.Select(item => item.FirstName).ToArray());
    }

    [Fact]
    public async Task DevelopmentDataSeeder_CreatesRunnableDemoWorkflow()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        await DevelopmentDataSeeder.SeedAsync(dbContext);

        var playBook = await dbContext.PlayBooks
            .Include(item => item.Steps)
            .Include(item => item.Transitions)
            .SingleOrDefaultAsync(item => item.Name == "Proposal approval and subscription flow");

        Assert.NotNull(playBook);
        Assert.Equal(PlayBookStatus.Active, playBook!.Status);
        Assert.Contains(playBook.Steps, step => step.IsStartStep && step.StepType == StepType.Trigger);
        Assert.Contains(playBook.Steps, step => step.StepType == StepType.Approval);
        Assert.NotEmpty(playBook.Transitions);

        var proposal = await dbContext.Proposals.SingleOrDefaultAsync();
        Assert.NotNull(proposal);
        Assert.Equal(ProposalStatus.Draft, proposal!.Status);
    }

    [Fact]
    public async Task TriggerAsync_StartsOnlyPlayBooksMatchingConfiguredEvent()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);
        var matchingStart = new PlayBookStep { Id = Guid.NewGuid(), Name = "Opportunity created", StepType = StepType.Trigger, IsStartStep = true, ConfigurationJson = "{\"event\":\"Opportunity Created\"}" };
        var matchingEnd = new PlayBookStep { Id = Guid.NewGuid(), Name = "Done", StepType = StepType.End, IsEndStep = true };
        var ignoredStart = new PlayBookStep { Id = Guid.NewGuid(), Name = "Proposal created", StepType = StepType.Trigger, IsStartStep = true, ConfigurationJson = "{\"event\":\"Proposal Created\"}" };
        var ignoredEnd = new PlayBookStep { Id = Guid.NewGuid(), Name = "Done", StepType = StepType.End, IsEndStep = true };
        var matching = new PlayBook.Domain.PlayBook { Id = Guid.NewGuid(), Name = "Opportunity events", Status = PlayBookStatus.Active, TriggerType = TriggerType.Event, Steps = [matchingStart, matchingEnd], Transitions = [new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = matchingStart.Id, ToStepId = matchingEnd.Id, Priority = 1 }] };
        var ignored = new PlayBook.Domain.PlayBook { Id = Guid.NewGuid(), Name = "Proposal events", Status = PlayBookStatus.Active, TriggerType = TriggerType.Event, Steps = [ignoredStart, ignoredEnd], Transitions = [new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = ignoredStart.Id, ToStepId = ignoredEnd.Id, Priority = 1 }] };
        dbContext.PlayBooks.AddRange(matching, ignored);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        var executions = await service.TriggerAsync("Opportunity Created", "Opportunity", Guid.NewGuid(), null);

        var execution = Assert.Single(executions);
        Assert.Equal(matching.Id, execution.PlayBookId);
        Assert.Equal(WorkflowStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task EventWorkflow_CreatesProposalFromConfiguredProductLines()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);
        await DevelopmentDataSeeder.SeedAsync(dbContext);
        var customer = await dbContext.Customers.SingleAsync();
        var employee = await dbContext.Employees.SingleAsync(item => item.FirstName == "Aditya");
        var products = await dbContext.Products.OrderBy(item => item.Name).Take(2).ToListAsync();
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, AssignedEmployeeId = employee.Id, Name = "Configured product sale", EstimatedValue = 1000m };
        dbContext.Opportunities.Add(opportunity);
        await dbContext.SaveChangesAsync();

        var eventPayload = JsonSerializer.SerializeToElement(new
        {
            products = new[]
            {
                new { productId = products[0].Id, quantity = 2, unitPrice = 100m, discountPercentage = 10m },
                new { productId = products[1].Id, quantity = 1, unitPrice = 90m, discountPercentage = 5m },
            }
        });
        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        await service.TriggerAsync("Opportunity Created", "Opportunity", opportunity.Id, eventPayload);

        var proposal = await dbContext.Proposals.Include(item => item.ProposalProducts).SingleAsync(item => item.OpportunityId == opportunity.Id);
        Assert.Equal(290m, proposal.SubTotal);
        Assert.Equal(24.5m, proposal.DiscountAmount);
        Assert.Equal(265.5m, proposal.TotalAmount);
        Assert.Equal(2, proposal.ProposalProducts.Count);
    }

    [Fact]
    public async Task StartAsync_SelectsConfiguredTrueBranch()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);
        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true };
        var condition = new PlayBookStep { Id = Guid.NewGuid(), Name = "Check amount", StepType = StepType.Condition };
        var trueEnd = new PlayBookStep { Id = Guid.NewGuid(), Name = "High value", StepType = StepType.End, IsEndStep = true };
        var falseEnd = new PlayBookStep { Id = Guid.NewGuid(), Name = "Standard value", StepType = StepType.End, IsEndStep = true };
        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(), Name = "Branching flow", Status = PlayBookStatus.Active, TriggerType = TriggerType.Manual,
            Steps = [start, condition, trueEnd, falseEnd],
            Transitions =
            [
                new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = start.Id, ToStepId = condition.Id, Priority = 1 },
                new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = condition.Id, ToStepId = trueEnd.Id, Label = "TRUE", Priority = 1, Condition = new Condition { Id = Guid.NewGuid(), StepId = condition.Id, Field = "amount", Operator = ConditionOperator.GreaterThan, Value = "100", DataType = "decimal" } },
                new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = condition.Id, ToStepId = falseEnd.Id, Label = "FALSE", Priority = 2, Condition = new Condition { Id = Guid.NewGuid(), StepId = condition.Id, Field = "amount", Operator = ConditionOperator.GreaterThan, Value = "100", DataType = "decimal" } },
            ]
        };
        dbContext.PlayBooks.Add(playBook);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        var execution = await service.StartAsync(new StartWorkflowRequest(playBook.Id, "Opportunity", Guid.NewGuid(), JsonSerializer.SerializeToElement(new { amount = 150 })));

        Assert.Equal(WorkflowStatus.Completed, execution.Status);
        Assert.Contains(await dbContext.WorkflowExecutionSteps.Where(step => step.WorkflowExecutionId == execution.Id).ToListAsync(), step => step.PlayBookStepId == trueEnd.Id);
    }

    [Fact]
    public void EngagementActivity_CanBeAssociatedWithCustomerOpportunityAndProposal()
    {
        var customerId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var activity = new EngagementActivity { Id = Guid.NewGuid(), CustomerId = customerId, OpportunityId = opportunityId, ProposalId = proposalId, Type = "Call", Subject = "Renewal follow-up" };

        Assert.Equal(customerId, activity.CustomerId);
        Assert.Equal(opportunityId, activity.OpportunityId);
        Assert.Equal(proposalId, activity.ProposalId);
    }

    [Fact]
    public async Task ResubmitAsync_AllowsRejectedProposalToRestartApprovalFlow()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var manager = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Aisha",
            LastName = "Manager",
            Email = "aisha@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Director", ApprovalLimit = 5000m, IsActive = true }
        };
        var anand = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Anand",
            LastName = "Boss",
            Email = "anand@demo.com",
            IsActive = true,
            Manager = manager,
            ManagerId = manager.Id,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Senior", ApprovalLimit = 1000m, IsActive = true }
        };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-100",
            OpportunityId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CreatedByEmployeeId = anand.Id,
            CreatedByEmployee = anand,
            Customer = new Customer { Id = Guid.NewGuid(), Name = "Contoso", Status = "Active" },
            Opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = Guid.Empty, Name = "New sale", EstimatedValue = 1500m },
            Status = ProposalStatus.Rejected,
            SubTotal = 1500m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 1500m,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        dbContext.Employees.Add(manager);
        dbContext.Employees.Add(anand);
        dbContext.Customers.Add(proposal.Customer);
        dbContext.Opportunities.Add(proposal.Opportunity);
        dbContext.Proposals.Add(proposal);
        await dbContext.SaveChangesAsync();

        var service = new ApprovalService(dbContext);

        var result = await service.ResubmitAsync(proposal.Id);

        Assert.Equal(ProposalStatus.PendingApproval, proposal.Status);
        Assert.Equal(ApprovalStatus.Pending, result.Status);
    }

    [Fact]
    public async Task ResumeAsync_WhenCustomerApprovesProposal_TransitionsToCustomerApproved()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "Contoso", Status = "Active" };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Renewal", EstimatedValue = 2000m, Status = OpportunityStatus.Proposal };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Harper",
            LastName = "Seller",
            Email = "harper@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Sales", ApprovalLimit = 5000m, IsActive = true }
        };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-200",
            OpportunityId = opportunity.Id,
            CustomerId = customer.Id,
            CreatedByEmployeeId = employee.Id,
            CreatedByEmployee = employee,
            Customer = customer,
            Opportunity = opportunity,
            Status = ProposalStatus.CustomerPending,
            SubTotal = 2000m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 2000m,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true, IsEndStep = false };
        var customerAction = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer approval", StepType = StepType.CustomerAction, IsStartStep = false, IsEndStep = false };
        var end = new PlayBookStep { Id = Guid.NewGuid(), Name = "End", StepType = StepType.End, IsEndStep = true };

        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(),
            Name = "Customer approval flow",
            Status = PlayBookStatus.Active,
            TriggerType = TriggerType.Manual,
            CreatedBy = "qa",
            Steps = [start, customerAction, end],
            Transitions =
            [
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = start.Id, ToStepId = customerAction.Id, Priority = 1 },
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = customerAction.Id, ToStepId = end.Id, Priority = 1 }
            ]
        };

        start.PlayBook = playBook;
        customerAction.PlayBook = playBook;
        end.PlayBook = playBook;

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            PlayBookId = playBook.Id,
            PlayBook = playBook,
            EntityType = "Proposal",
            EntityId = proposal.Id,
            CurrentStepId = customerAction.Id,
            Status = WorkflowStatus.Waiting,
            StartedAt = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        dbContext.Employees.Add(employee);
        dbContext.Opportunities.Add(opportunity);
        dbContext.Proposals.Add(proposal);
        dbContext.PlayBooks.Add(playBook);
        dbContext.WorkflowExecutions.Add(execution);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        var result = await service.ResumeAsync(execution.Id, new { decision = "Approved" });

        Assert.Equal(ProposalStatus.CustomerApproved, proposal.Status);
        Assert.Equal(WorkflowStatus.Completed, result.Status);
    }

    [Fact]
    public async Task ResumeAsync_WhenCustomerApprovesProposal_CreatesOrderAndClosesOpportunity()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "Contoso", Status = "Active" };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Renewal", EstimatedValue = 3500m, Status = OpportunityStatus.Proposal };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Harper",
            LastName = "Seller",
            Email = "harper2@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Sales", ApprovalLimit = 5000m, IsActive = true }
        };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-300",
            OpportunityId = opportunity.Id,
            CustomerId = customer.Id,
            CreatedByEmployeeId = employee.Id,
            CreatedByEmployee = employee,
            Customer = customer,
            Opportunity = opportunity,
            Status = ProposalStatus.CustomerPending,
            SubTotal = 3500m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 3500m,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true, IsEndStep = false };
        var customerAction = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer approval", StepType = StepType.CustomerAction, IsStartStep = false, IsEndStep = false };
        var end = new PlayBookStep { Id = Guid.NewGuid(), Name = "End", StepType = StepType.End, IsEndStep = true };
        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(),
            Name = "Customer approval flow",
            Status = PlayBookStatus.Active,
            TriggerType = TriggerType.Manual,
            CreatedBy = "qa",
            Steps = [start, customerAction, end],
            Transitions =
            [
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = start.Id, ToStepId = customerAction.Id, Priority = 1 },
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = customerAction.Id, ToStepId = end.Id, Priority = 1 }
            ]
        };
        start.PlayBook = playBook;
        customerAction.PlayBook = playBook;
        end.PlayBook = playBook;

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            PlayBookId = playBook.Id,
            PlayBook = playBook,
            EntityType = "Proposal",
            EntityId = proposal.Id,
            CurrentStepId = customerAction.Id,
            Status = WorkflowStatus.Waiting,
            StartedAt = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        dbContext.Employees.Add(employee);
        dbContext.Opportunities.Add(opportunity);
        dbContext.Proposals.Add(proposal);
        dbContext.PlayBooks.Add(playBook);
        dbContext.WorkflowExecutions.Add(execution);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        await service.ResumeAsync(execution.Id, new { decision = "Approved" });

        var createdOrder = await dbContext.Orders.SingleOrDefaultAsync(o => o.ProposalId == proposal.Id);
        Assert.NotNull(createdOrder);
        Assert.Equal(OrderStatus.Pending, createdOrder!.Status);
        Assert.Equal(3500m, createdOrder.TotalAmount);
        Assert.Equal(OpportunityStatus.Won, opportunity.Status);
    }

    [Fact]
    public async Task ResumeAsync_WhenCustomerRejectsProposal_ReopensOpportunityForCorrection()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "Contoso", Status = "Active" };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Renewal", EstimatedValue = 2000m, Status = OpportunityStatus.Proposal };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Harper",
            LastName = "Seller",
            Email = "harper-reject@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Sales", ApprovalLimit = 5000m, IsActive = true }
        };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-210",
            OpportunityId = opportunity.Id,
            CustomerId = customer.Id,
            CreatedByEmployeeId = employee.Id,
            CreatedByEmployee = employee,
            Customer = customer,
            Opportunity = opportunity,
            Status = ProposalStatus.CustomerPending,
            SubTotal = 2000m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 2000m,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true, IsEndStep = false };
        var customerAction = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer approval", StepType = StepType.CustomerAction, IsStartStep = false, IsEndStep = false };
        var end = new PlayBookStep { Id = Guid.NewGuid(), Name = "End", StepType = StepType.End, IsEndStep = true };
        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(),
            Name = "Customer correction flow",
            Status = PlayBookStatus.Active,
            TriggerType = TriggerType.Manual,
            CreatedBy = "qa",
            Steps = [start, customerAction, end],
            Transitions =
            [
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = start.Id, ToStepId = customerAction.Id, Priority = 1 },
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = customerAction.Id, ToStepId = end.Id, Priority = 1 }
            ]
        };
        start.PlayBook = playBook;
        customerAction.PlayBook = playBook;
        end.PlayBook = playBook;

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            PlayBookId = playBook.Id,
            PlayBook = playBook,
            EntityType = "Proposal",
            EntityId = proposal.Id,
            CurrentStepId = customerAction.Id,
            Status = WorkflowStatus.Waiting,
            StartedAt = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        dbContext.Employees.Add(employee);
        dbContext.Opportunities.Add(opportunity);
        dbContext.Proposals.Add(proposal);
        dbContext.PlayBooks.Add(playBook);
        dbContext.WorkflowExecutions.Add(execution);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        await service.ResumeAsync(execution.Id, new { decision = "Rejected" });

        Assert.Equal(ProposalStatus.CustomerRejected, proposal.Status);
        Assert.Equal(OpportunityStatus.Proposal, opportunity.Status);
        Assert.Equal(WorkflowStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task ResumeAsync_WhenCustomerApprovesProposal_CreatesSubscriptionsForOrderProducts()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "Contoso", Status = "Active" };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Renewal", EstimatedValue = 2000m, Status = OpportunityStatus.Proposal };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Harper",
            LastName = "Seller",
            Email = "harper-subscriptions@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Sales", ApprovalLimit = 5000m, IsActive = true }
        };
        var product = new Product { Id = Guid.NewGuid(), Name = "Internet 100 Mbps", Category = "Connectivity", Price = 1499m, IsActive = true };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-500",
            OpportunityId = opportunity.Id,
            CustomerId = customer.Id,
            CreatedByEmployeeId = employee.Id,
            CreatedByEmployee = employee,
            Customer = customer,
            Opportunity = opportunity,
            Status = ProposalStatus.CustomerPending,
            SubTotal = 1499m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 1499m,
            ValidUntil = DateTime.UtcNow.AddDays(30),
            ProposalProducts =
            [
                new ProposalProduct
                {
                    Id = Guid.NewGuid(),
                    ProposalId = Guid.Empty,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = 1499m,
                    DiscountPercentage = 0m,
                    DiscountAmount = 0m,
                    TotalPrice = 1499m
                }
            ]
        };

        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true, IsEndStep = false };
        var customerAction = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer approval", StepType = StepType.CustomerAction, IsStartStep = false, IsEndStep = false };
        var end = new PlayBookStep { Id = Guid.NewGuid(), Name = "End", StepType = StepType.End, IsEndStep = true };
        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(),
            Name = "Subscription flow",
            Status = PlayBookStatus.Active,
            TriggerType = TriggerType.Manual,
            CreatedBy = "qa",
            Steps = [start, customerAction, end],
            Transitions =
            [
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = start.Id, ToStepId = customerAction.Id, Priority = 1 },
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = customerAction.Id, ToStepId = end.Id, Priority = 1 }
            ]
        };
        start.PlayBook = playBook;
        customerAction.PlayBook = playBook;
        end.PlayBook = playBook;

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            PlayBookId = playBook.Id,
            PlayBook = playBook,
            EntityType = "Proposal",
            EntityId = proposal.Id,
            CurrentStepId = customerAction.Id,
            Status = WorkflowStatus.Waiting,
            StartedAt = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        dbContext.Employees.Add(employee);
        dbContext.Products.Add(product);
        dbContext.Opportunities.Add(opportunity);
        dbContext.Proposals.Add(proposal);
        dbContext.PlayBooks.Add(playBook);
        dbContext.WorkflowExecutions.Add(execution);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        await service.ResumeAsync(execution.Id, new { decision = "Approved" });

        var subscriptions = await dbContext.Subscriptions.Where(s => s.CustomerId == customer.Id).ToListAsync();
        Assert.Single(subscriptions);
        Assert.Equal(product.Id, subscriptions[0].ProductId);
        Assert.Equal(SubscriptionStatus.Active, subscriptions[0].Status);
        Assert.True(subscriptions[0].EndDate > subscriptions[0].StartDate);
    }

    [Fact]
    public async Task ResumeAsync_WhenCustomerApprovesProposal_RenewsExistingSubscriptionForSameProduct()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var customer = new Customer { Id = Guid.NewGuid(), Name = "Contoso", Status = "Active" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Internet 100 Mbps", Category = "Connectivity", Price = 1499m, IsActive = true };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Renewal", EstimatedValue = 1499m, Status = OpportunityStatus.Proposal };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Harper",
            LastName = "Seller",
            Email = "harper-renewal@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Sales", ApprovalLimit = 5000m, IsActive = true }
        };
        var existingSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProductId = product.Id,
            StartDate = DateTime.UtcNow.AddDays(-200),
            EndDate = DateTime.UtcNow.AddDays(-10),
            Amount = 1499m,
            Status = SubscriptionStatus.Active
        };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-600",
            OpportunityId = opportunity.Id,
            CustomerId = customer.Id,
            CreatedByEmployeeId = employee.Id,
            CreatedByEmployee = employee,
            Customer = customer,
            Opportunity = opportunity,
            Status = ProposalStatus.CustomerPending,
            SubTotal = 1499m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 1499m,
            ValidUntil = DateTime.UtcNow.AddDays(30),
            ProposalProducts =
            [
                new ProposalProduct
                {
                    Id = Guid.NewGuid(),
                    ProposalId = Guid.Empty,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = 1499m,
                    DiscountPercentage = 0m,
                    DiscountAmount = 0m,
                    TotalPrice = 1499m
                }
            ]
        };

        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true, IsEndStep = false };
        var customerAction = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer approval", StepType = StepType.CustomerAction, IsStartStep = false, IsEndStep = false };
        var end = new PlayBookStep { Id = Guid.NewGuid(), Name = "End", StepType = StepType.End, IsEndStep = true };
        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(),
            Name = "Renewal flow",
            Status = PlayBookStatus.Active,
            TriggerType = TriggerType.Manual,
            CreatedBy = "qa",
            Steps = [start, customerAction, end],
            Transitions =
            [
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = start.Id, ToStepId = customerAction.Id, Priority = 1 },
                new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = Guid.Empty, FromStepId = customerAction.Id, ToStepId = end.Id, Priority = 1 }
            ]
        };
        start.PlayBook = playBook;
        customerAction.PlayBook = playBook;
        end.PlayBook = playBook;

        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            PlayBookId = playBook.Id,
            PlayBook = playBook,
            EntityType = "Proposal",
            EntityId = proposal.Id,
            CurrentStepId = customerAction.Id,
            Status = WorkflowStatus.Waiting,
            StartedAt = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        dbContext.Employees.Add(employee);
        dbContext.Products.Add(product);
        dbContext.Subscriptions.Add(existingSubscription);
        dbContext.Opportunities.Add(opportunity);
        dbContext.Proposals.Add(proposal);
        dbContext.PlayBooks.Add(playBook);
        dbContext.WorkflowExecutions.Add(execution);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        await service.ResumeAsync(execution.Id, new { decision = "Approved" });

        var activeSubscriptions = await dbContext.Subscriptions
            .Where(item => item.CustomerId == customer.Id && item.ProductId == product.Id && item.Status == SubscriptionStatus.Active)
            .ToListAsync();

        Assert.Single(activeSubscriptions);
        Assert.True(activeSubscriptions[0].EndDate > activeSubscriptions[0].StartDate);
        Assert.Equal(SubscriptionStatus.Renewed, existingSubscription.Status);
    }

    [Fact]
    public void UpdateSubscriptionStatus_WhenWithinThirtyDaysOfExpiry_MarksSubscriptionExpiring()
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(-300),
            EndDate = DateTime.UtcNow.AddDays(10),
            Amount = 1499m,
            Status = SubscriptionStatus.Active
        };

        WorkflowExecutionService.UpdateSubscriptionStatus(subscription, DateTime.UtcNow);

        Assert.Equal(SubscriptionStatus.Expiring, subscription.Status);
    }

    [Fact]
    public void UpdateSubscriptionStatus_WhenPastExpiry_MarksSubscriptionExpired()
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(-365),
            EndDate = DateTime.UtcNow.AddDays(-2),
            Amount = 1499m,
            Status = SubscriptionStatus.Active
        };

        WorkflowExecutionService.UpdateSubscriptionStatus(subscription, DateTime.UtcNow);

        Assert.Equal(SubscriptionStatus.Expired, subscription.Status);
    }

    [Fact]
    public void RenewExpiringSubscription_CreatesFreshActiveSubscriptionAndMarksPreviousAsRenewed()
    {
        var now = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc);
        var oldSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            StartDate = now.AddDays(-350),
            EndDate = now.AddDays(10),
            Amount = 1499m,
            Status = SubscriptionStatus.Expiring
        };

        var renewed = WorkflowExecutionService.RenewExpiringSubscription(oldSubscription, now);

        Assert.NotNull(renewed);
        Assert.Equal(SubscriptionStatus.Renewed, oldSubscription.Status);
        Assert.Equal(SubscriptionStatus.Active, renewed.Status);
        Assert.Equal(oldSubscription.CustomerId, renewed.CustomerId);
        Assert.Equal(oldSubscription.ProductId, renewed.ProductId);
        Assert.Equal(now, renewed.StartDate);
        Assert.Equal(now.AddYears(1), renewed.EndDate);
    }

    [Fact]
    public async Task RequestAsync_AllowsCustomerRejectedProposalToResubmitForApproval()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new PlayBookDbContext(options);

        var manager = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Aisha",
            LastName = "Manager",
            Email = "aisha3@demo.com",
            IsActive = true,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Director", ApprovalLimit = 5000m, IsActive = true }
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Tara",
            LastName = "Seller",
            Email = "tara3@demo.com",
            IsActive = true,
            Manager = manager,
            ManagerId = manager.Id,
            EmployeeGrade = new EmployeeGrade { Id = Guid.NewGuid(), Name = "Sales", ApprovalLimit = 1000m, IsActive = true }
        };
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Northwind", Status = "Active" };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Expansion", EstimatedValue = 1250m, Status = OpportunityStatus.Proposal };
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "P-400",
            OpportunityId = opportunity.Id,
            CustomerId = customer.Id,
            CreatedByEmployeeId = employee.Id,
            CreatedByEmployee = employee,
            Customer = customer,
            Opportunity = opportunity,
            Status = ProposalStatus.CustomerRejected,
            SubTotal = 1250m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            TotalAmount = 1250m,
            ValidUntil = DateTime.UtcNow.AddDays(20)
        };

        dbContext.Employees.Add(manager);
        dbContext.Employees.Add(employee);
        dbContext.Customers.Add(customer);
        dbContext.Opportunities.Add(opportunity);
        dbContext.Proposals.Add(proposal);
        await dbContext.SaveChangesAsync();

        var service = new ApprovalService(dbContext);
        var result = await service.RequestAsync(proposal.Id);

        Assert.Equal(ProposalStatus.PendingApproval, proposal.Status);
        Assert.Equal(ApprovalStatus.Pending, result.Status);
    }

    [Fact]
    public async Task TriggerAsync_FiltersByAssignedEmployeeAndLeavesUnrestrictedPlayBooksWorking()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var dbContext = new PlayBookDbContext(options);
        var employee = new Employee { Id = Guid.NewGuid(), FirstName = "Assigned", LastName = "Seller", Email = "assigned-filter@test.local", IsActive = true };
        var opportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), AssignedEmployeeId = employee.Id, Name = "Filtered opportunity" };
        var otherOpportunity = new Opportunity { Id = Guid.NewGuid(), CustomerId = opportunity.CustomerId, AssignedEmployeeId = Guid.NewGuid(), Name = "Other opportunity" };
        dbContext.Employees.Add(employee);
        dbContext.Opportunities.AddRange(opportunity, otherOpportunity);
        var start = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", StepType = StepType.Trigger, IsStartStep = true, ConfigurationJson = "{\"event\":\"Opportunity Created\",\"employeeId\":\"" + employee.Id + "\"}" };
        var end = new PlayBookStep { Id = Guid.NewGuid(), Name = "End", StepType = StepType.End, IsEndStep = true };
        var playBook = new PlayBook.Domain.PlayBook { Id = Guid.NewGuid(), Name = "Employee trigger", Status = PlayBookStatus.Active, TriggerType = TriggerType.Event, Steps = [start, end], Transitions = [new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = start.Id, ToStepId = end.Id }] };
        dbContext.PlayBooks.Add(playBook);
        await dbContext.SaveChangesAsync();
        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));

        var matching = await service.TriggerAsync("Opportunity Created", "Opportunity", opportunity.Id, null);
        var nonMatching = await service.TriggerAsync("Opportunity Created", "Opportunity", otherOpportunity.Id, null);

        Assert.Single(matching);
        Assert.Empty(nonMatching);
    }

    [Fact]
    public async Task RenewalProcessor_CreatesEachConfiguredReminderOnlyOnce()
    {
        var options = new DbContextOptionsBuilder<PlayBookDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var dbContext = new PlayBookDbContext(options);
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Renewal customer" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Annual plan", Price = 100m, PlanDurationMonths = 12 };
        var now = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        dbContext.Customers.Add(customer);
        dbContext.Products.Add(product);
        dbContext.Subscriptions.Add(new Subscription { Id = Guid.NewGuid(), CustomerId = customer.Id, ProductId = product.Id, StartDate = now.AddMonths(-6), EndDate = now.AddDays(60), Amount = 100m });
        await dbContext.SaveChangesAsync();
        var service = new WorkflowExecutionService(dbContext, new ConditionEvaluator(), new ApprovalService(dbContext));
        var processor = new RenewalProcessor(dbContext, service);

        var firstRun = await processor.ProcessAsync(now, [90, 60, 30]);
        var secondRun = await processor.ProcessAsync(now, [90, 60, 30]);

        Assert.Equal(2, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Equal(2, await dbContext.RenewalReminders.CountAsync());
        Assert.Equal(2, await dbContext.EngagementActivities.CountAsync());
    }
}
