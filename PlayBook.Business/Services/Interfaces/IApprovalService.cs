using PlayBook.Business.DTOs.Approval;

namespace PlayBook.Business.Services.Interfaces;

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