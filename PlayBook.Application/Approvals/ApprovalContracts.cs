using PlayBook.Domain;

namespace PlayBook.Application.Approvals;

public sealed record ApprovalDto(
    Guid Id,
    Guid ProposalId,
    Guid? WorkflowExecutionId,
    Guid ApproverEmployeeId,
    string ApproverName,
    int ApprovalLevel,
    int ProposalRevision,
    ApprovalStatus Status,
    string? Comments,
    DateTime RequestedAt,
    DateTime? RespondedAt);

public sealed record ApprovalDecisionRequest(Guid ApproverEmployeeId, ApprovalStatus Decision, string? Comments);

public interface IApprovalService
{
    Task<ApprovalDto> RequestAsync(Guid proposalId, Guid? workflowExecutionId = null, CancellationToken cancellationToken = default);
    Task<ApprovalDto> ResubmitAsync(Guid proposalId, Guid? workflowExecutionId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalDto>> GetForProposalAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task<ApprovalDto> DecideAsync(Guid approvalId, ApprovalDecisionRequest request, CancellationToken cancellationToken = default);
}
