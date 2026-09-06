using PlayBook.Domain;

namespace PlayBook.Business.BusinessModels.ResponseDTOs.CRMResponseDTOs;

public sealed record EmployeeGradeDto(
    Guid Id,
    string Name,
    string? Description,
    decimal ApprovalLimit,
    bool IsActive);

public sealed record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Guid? EmployeeGradeId,
    Guid? ManagerId,
    EmployeeRole Role,
    bool IsActive);

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Address,
    string Status);

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string? Category,
    decimal Price,
    bool IsActive,
    int? PlanDurationMonths = null);

public sealed record OpportunityDto(
    Guid Id,
    Guid CustomerId,
    Guid? AssignedEmployeeId,
    string Name,
    string? Description,
    decimal EstimatedValue,
    OpportunityStatus Status,
    DateTime? ExpectedCloseDate);

public sealed record ProposalDto(
    Guid Id,
    Guid OpportunityId,
    Guid CustomerId,
    Guid CreatedByEmployeeId,
    string ProposalNumber,
    ProposalStatus Status,
    decimal SubTotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount,
    DateTime? ValidUntil,
    decimal VoucherDiscountAmount = 0,
    string? VoucherCode = null,
    int Revision = 1);

public sealed record ProposalProductDto(
    Guid Id,
    Guid ProposalId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalPrice);

public sealed record OrderDto(
    Guid Id,
    Guid ProposalId,
    Guid CustomerId,
    Guid? AssignedEmployeeId,
    string OrderNumber,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime OrderDate,
    decimal DiscountAmount = 0);

public sealed record OrderProductDto(
    Guid Id,
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal TotalPrice);

public sealed record SubscriptionDto(
    Guid Id,
    Guid CustomerId,
    Guid ProductId,
    DateTime StartDate,
    DateTime EndDate,
    decimal Amount,
    SubscriptionStatus Status);

public sealed record EngagementActivityDto(
    Guid Id,
    Guid CustomerId,
    Guid? EmployeeId,
    Guid? OpportunityId,
    Guid? ProposalId,
    string Type,
    string? Subject,
    string? Description,
    DateTime ActivityDate);

public sealed record ConversationDto(
    Guid Id,
    Guid CustomerId,
    Guid? EmployeeId,
    Guid? OpportunityId,
    string Message,
    string Channel,
    DateTime CreatedAt);

public sealed record VoucherDto(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    bool IsActive,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    decimal? MinimumAmount,
    bool Stackable);