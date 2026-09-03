using PlayBook.Domain;

namespace PlayBook.Business.DTOs.Approval;

public sealed record ApprovalResponseDto(
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