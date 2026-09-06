using PlayBook.Domain;

namespace PlayBook.Business.BusinessModels.ResponseDTOs.ApprovalResponseDTOs;

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