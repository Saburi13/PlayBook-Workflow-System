using PlayBook.Domain;

namespace PlayBook.Business.BusinessModels.RequestDTOs.ApprovalRequestDTOs;

public sealed record ApprovalDecisionRequestDto(
    Guid ApproverEmployeeId,
    ApprovalStatus Decision,
    string? Comments);