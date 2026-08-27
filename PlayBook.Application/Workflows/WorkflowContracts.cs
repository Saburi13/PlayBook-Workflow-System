using PlayBook.Domain;

namespace PlayBook.Application.Workflows;

public sealed record StartWorkflowRequest(Guid PlayBookId, string EntityType, Guid EntityId, object? Payload);
public sealed record WorkflowExecutionDto(Guid Id, Guid PlayBookId, string EntityType, Guid EntityId, Guid? CurrentStepId, WorkflowStatus Status, string? ErrorMessage);

public interface IWorkflowExecutionService
{
    Task<IReadOnlyList<WorkflowExecutionDto>> TriggerAsync(string eventName, string entityType, Guid entityId, object? payload, CancellationToken cancellationToken = default);
    Task<WorkflowExecutionDto> StartAsync(StartWorkflowRequest request, CancellationToken cancellationToken = default);
    Task<WorkflowExecutionDto> ResumeAsync(Guid executionId, object? payload, CancellationToken cancellationToken = default);
}
