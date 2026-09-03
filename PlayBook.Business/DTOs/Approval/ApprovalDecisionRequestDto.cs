using PlayBook.Domain;

namespace PlayBook.Business.DTOs.Approval;

public sealed record ApprovalDecisionRequestDto(
    Guid ApproverEmployeeId,
    ApprovalStatus Decision,
    string? Comments);