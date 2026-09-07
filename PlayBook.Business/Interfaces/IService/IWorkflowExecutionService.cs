using PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.WorkflowResponseDTOs;


namespace PlayBook.Business.Interfaces.IService;

public interface IWorkflowExecutionService
{
    Task<IReadOnlyList<WorkflowExecutionDto>> TriggerAsync(
        string eventName,
        string entityType,
        Guid entityId,
        object? payload,
        CancellationToken cancellationToken = default);

    Task<WorkflowExecutionDto> StartAsync(
        StartWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkflowExecutionDto> ResumeAsync(
        Guid executionId,
        object? payload,
        CancellationToken cancellationToken = default);
}
