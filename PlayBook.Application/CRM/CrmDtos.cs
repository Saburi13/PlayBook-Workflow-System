using System.ComponentModel.DataAnnotations;
using PlayBook.Domain;

namespace PlayBook.Application.CRM;

public sealed record EmployeeGradeDto(Guid Id, string Name, string? Description, decimal ApprovalLimit, bool IsActive);
public sealed record EmployeeDto(Guid Id, string FirstName, string LastName, string Email, string? Phone, Guid? EmployeeGradeId, Guid? ManagerId, EmployeeRole Role, bool IsActive);
public sealed record CustomerDto(Guid Id, string Name, string? Email, string? Phone, string? Company, string? Address, string Status);
public sealed record ProductDto(Guid Id, string Name, string? Description, string? Category, decimal Price, bool IsActive, int? PlanDurationMonths = null);
public sealed record OpportunityDto(Guid Id, Guid CustomerId, Guid? AssignedEmployeeId, string Name, string? Description, decimal EstimatedValue, OpportunityStatus Status, DateTime? ExpectedCloseDate);
public sealed record ProposalDto(Guid Id, Guid OpportunityId, Guid CustomerId, Guid CreatedByEmployeeId, string ProposalNumber, ProposalStatus Status, decimal SubTotal, decimal DiscountPercentage, decimal DiscountAmount, decimal TotalAmount, DateTime? ValidUntil, decimal VoucherDiscountAmount = 0, string? VoucherCode = null, int Revision = 1);
public sealed record ProposalProductDto(Guid Id, Guid ProposalId, Guid ProductId, int Quantity, decimal UnitPrice, decimal DiscountPercentage, decimal DiscountAmount, decimal TotalPrice);
public sealed record OrderDto(Guid Id, Guid ProposalId, Guid CustomerId, Guid? AssignedEmployeeId, string OrderNumber, OrderStatus Status, decimal TotalAmount, DateTime OrderDate, decimal DiscountAmount = 0);
public sealed record OrderProductDto(Guid Id, Guid OrderId, Guid ProductId, int Quantity, decimal UnitPrice, decimal Discount, decimal TotalPrice);
public sealed record SubscriptionDto(Guid Id, Guid CustomerId, Guid ProductId, DateTime StartDate, DateTime EndDate, decimal Amount, SubscriptionStatus Status);
public sealed record EngagementActivityDto(Guid Id, Guid CustomerId, Guid? EmployeeId, Guid? OpportunityId, Guid? ProposalId, string Type, string? Subject, string? Description, DateTime ActivityDate);
public sealed record ConversationDto(Guid Id, Guid CustomerId, Guid? EmployeeId, Guid? OpportunityId, string Message, string Channel, DateTime CreatedAt);
public sealed record VoucherDto(Guid Id, string Code, DiscountType DiscountType, decimal DiscountValue, bool IsActive, DateTime? ValidFrom, DateTime? ValidUntil, decimal? MinimumAmount, bool Stackable);

public sealed class EmployeeGradeRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    [Range(0, 999999999)] public decimal ApprovalLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class EmployeeRequest
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(320)] public string Email { get; set; } = string.Empty;
    [MaxLength(50)] public string? Phone { get; set; }
    public Guid? EmployeeGradeId { get; set; }
    public Guid? ManagerId { get; set; }
    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;
    public bool IsActive { get; set; } = true;
}

public sealed class CustomerRequest
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [EmailAddress, MaxLength(320)] public string? Email { get; set; }
    [MaxLength(50)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Company { get; set; }
    [MaxLength(1000)] public string? Address { get; set; }
    [Required, MaxLength(50)] public string Status { get; set; } = "Active";
}

public sealed class ProductRequest
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    [MaxLength(100)] public string? Category { get; set; }
    [Range(0, 999999999)] public decimal Price { get; set; }
    public int? PlanDurationMonths { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OpportunityRequest
{
    [Required] public Guid CustomerId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [Range(0, 999999999)] public decimal EstimatedValue { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.New;
    public DateTime? ExpectedCloseDate { get; set; }
}

public sealed class ProposalRequest
{
    [Required] public Guid OpportunityId { get; set; }
    [Required] public Guid CustomerId { get; set; }
    [Required] public Guid CreatedByEmployeeId { get; set; }
    [Required, MaxLength(100)] public string ProposalNumber { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; } = ProposalStatus.Draft;
    [Range(0, 999999999)] public decimal SubTotal { get; set; }
    [Range(0, 100)] public decimal DiscountPercentage { get; set; }
    [Range(0, 999999999)] public decimal DiscountAmount { get; set; }
    [Range(0, 999999999)] public decimal TotalAmount { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? VoucherCode { get; set; }
    public List<ProposalProductRequest> Products { get; set; } = [];
}

public sealed class CorrectProposalRequest
{
    [MaxLength(2000)] public string? Reason { get; set; }
}

public sealed class ProposalProductRequest
{
    [Required] public Guid ProductId { get; set; }
    [Range(1, 1000000)] public int Quantity { get; set; }
    [Range(0, 999999999)] public decimal UnitPrice { get; set; }
    [Range(0, 100)] public decimal DiscountPercentage { get; set; }
    [Range(0, 999999999)] public decimal DiscountAmount { get; set; }
    [Range(0, 999999999)] public decimal TotalPrice { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }
}

public sealed class OrderRequest
{
    [Required] public Guid ProposalId { get; set; }
    [Required] public Guid CustomerId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    [Required, MaxLength(100)] public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    [Range(0, 999999999)] public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}

public sealed class OrderProductRequest
{
    [Required] public Guid ProductId { get; set; }
    [Range(1, 1000000)] public int Quantity { get; set; }
    [Range(0, 999999999)] public decimal UnitPrice { get; set; }
    [Range(0, 999999999)] public decimal Discount { get; set; }
    [Range(0, 999999999)] public decimal TotalPrice { get; set; }
}

public sealed class EngagementActivityRequest
{
    [Required] public Guid CustomerId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? ProposalId { get; set; }
    [Required, MaxLength(100)] public string Type { get; set; } = string.Empty;
    [MaxLength(200)] public string? Subject { get; set; }
    [MaxLength(4000)] public string? Description { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
}

public sealed class ConversationRequest
{
    [Required] public Guid CustomerId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? OpportunityId { get; set; }
    [Required, MaxLength(4000)] public string Message { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Channel { get; set; } = "Internal";
}

public sealed class VoucherRequest
{
    [Required, MaxLength(100)] public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    [Range(0, 999999999)] public decimal DiscountValue { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    [Range(0, 999999999)] public decimal? MinimumAmount { get; set; }
    public bool Stackable { get; set; }
}
