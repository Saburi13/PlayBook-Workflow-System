using Microsoft.EntityFrameworkCore;
using PlayBook.Application.Approvals;
using PlayBook.Application.Interfaces;
using PlayBook.Application.Workflows;
using PlayBook.Domain;
using PlayBook.Infrastructure.Data;

namespace PlayBook.Infrastructure.Workflows;

public sealed class WorkflowExecutionService(
    PlayBookDbContext dbContext,
    IConditionEvaluator conditionEvaluator,
    IApprovalService approvalService) : IWorkflowExecutionService
{
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
        await ProcessAsync(execution, playBook, startStep, request.Payload, false, cancellationToken);
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

        execution.Status = WorkflowStatus.Running;
        await ProcessAsync(execution, execution.PlayBook, currentStep, payload, true, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(execution);
    }

    private async Task ProcessAsync(WorkflowExecution execution, PlayBook.Domain.PlayBook playBook, PlayBookStep? currentStep, object? payload, bool skipBlockingStep, CancellationToken cancellationToken)
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
                .FirstOrDefault(item => item.Condition is null || conditionEvaluator.Evaluate(item.Condition.Field, item.Condition.Operator.ToString(), item.Condition.Value, payload, item.Condition.DataType));

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

    private static WorkflowExecutionDto ToDto(WorkflowExecution execution) =>
        new(execution.Id, execution.PlayBookId, execution.EntityType, execution.EntityId, execution.CurrentStepId, execution.Status, execution.ErrorMessage);
}
