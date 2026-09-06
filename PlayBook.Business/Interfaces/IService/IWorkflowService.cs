using PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.WorkflowResponseDTOs;

namespace PlayBook.Business.Interfaces.IService;

public interface IWorkflowService
{
    Task<IReadOnlyList<WorkflowPlayBookSummaryDto>> GetPlayBooksAsync(
        CancellationToken cancellationToken = default);

    Task<WorkflowPlayBookDto?> GetPlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WorkflowPlayBookSummaryDto> CreatePlayBookAsync(
        CreatePlayBookRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkflowPlayBookSummaryDto?> UpdatePlayBookAsync(
        Guid id,
        CreatePlayBookRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkflowPlayBookSummaryDto?> ActivatePlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WorkflowExecutionDto?> GetExecutionAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}