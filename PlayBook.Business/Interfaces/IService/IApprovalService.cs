using PlayBook.Business.BusinessModels.RequestDTOs.ApprovalRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.ApprovalResponseDTOs;

namespace PlayBook.Business.Interfaces.IService;

public interface IApprovalService
{
    Task<ApprovalResponseDto> RequestAsync(
        Guid proposalId,
        Guid? workflowExecutionId = null,
        CancellationToken cancellationToken = default);

    Task<ApprovalResponseDto> ResubmitAsync(
        Guid proposalId,
        Guid? workflowExecutionId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApprovalResponseDto>> GetForProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<ApprovalResponseDto> DecideAsync(
        Guid approvalId,
        ApprovalDecisionRequestDto request,
        CancellationToken cancellationToken = default);
}