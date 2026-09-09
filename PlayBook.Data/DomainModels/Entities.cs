using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayBook.Domain;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class EmployeeGrade : AuditableEntity
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal ApprovalLimit { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } =
        new List<Employee>();
}

public class Employee : AuditableEntity
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public Guid? EmployeeGradeId { get; set; }

    public Guid? ManagerId { get; set; }

    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

    public bool IsActive { get; set; } = true;

    public EmployeeGrade? EmployeeGrade { get; set; }

    public Employee? Manager { get; set; }

    public ICollection<Employee> DirectReports { get; set; } =
        new List<Employee>();

    public ICollection<Opportunity> Opportunities { get; set; } =
        new List<Opportunity>();

    public ICollection<Proposal> Proposals { get; set; } =
        new List<Proposal>();
}

public class Customer : AuditableEntity
{
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Company { get; set; }

    public string? Address { get; set; }

    public string Status { get; set; } = "Active";

    // =========================================================
    // AMS Account fields
    // =========================================================

    public string? AutoGenrateAccountId { get; set; }

    public string? RegisteredMobileNumber { get; set; }

    public string? SecondMobileNumber { get; set; }

    public string? Website { get; set; }

    public string? AccountProfileImg { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? IncorporationDate { get; set; }

    public DateTime? AccountSince { get; set; }

    public string? EmployeeCount { get; set; }

    public bool KeyAccount { get; set; } = false;

    public string? ReferralAccountId { get; set; }

    public string? ParentAccountId { get; set; }

    public Guid? RegionId { get; set; }

    public Guid? CurrencyId { get; set; }

    public string? DefaultCurrencySymbol { get; set; }

    public string? ConvertCurrencySymbol { get; set; }

    // =========================================================
    // Existing PlayBook relationships
    // =========================================================

    public ICollection<Opportunity> Opportunities { get; set; } =
        new List<Opportunity>();

    public ICollection<Proposal> Proposals { get; set; } =
        new List<Proposal>();

    public ICollection<Order> Orders { get; set; } =
        new List<Order>();

    public ICollection<EngagementActivity> Activities { get; set; } =
        new List<EngagementActivity>();

    public ICollection<Conversation> Conversations { get; set; } =
        new List<Conversation>();

    public ICollection<Subscription> Subscriptions { get; set; } =
        new List<Subscription>();

}




public class Product : AuditableEntity
{
    public Guid Id { get; set; }

    // =========================================================
    // Existing PlayBook fields
    // =========================================================

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int? PlanDurationMonths { get; set; }

    public bool IsActive { get; set; } = true;

    // =========================================================
    // AMS Product fields
    // =========================================================

    public string? ProductCode { get; set; }

    public string? AutoGenratedProductId { get; set; }

    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsFlexPrice { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public bool Status { get; set; } = true;

    public DurationType DurationType { get; set; }

    public int ValidityDuration { get; set; }

    // =========================================================
    // Existing relationships
    // =========================================================

    public ICollection<ProposalProduct> ProposalProducts { get; set; } =
        new List<ProposalProduct>();

    public ICollection<OrderProduct> OrderProducts { get; set; } =
        new List<OrderProduct>();

    public ICollection<Subscription> Subscriptions { get; set; } =
        new List<Subscription>();
}

public class Opportunity : AuditableEntity
{
    public Guid Id { get; set; }

    // =========================================================
    // Existing PlayBook fields
    // =========================================================

    public Guid CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    public string? AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    public Account? Account { get; set; }

    public Guid? AssignedEmployeeId { get; set; }

    [ForeignKey(nameof(AssignedEmployeeId))]
    public Employee? AssignedEmployee { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; }

    public OpportunityStatus Status { get; set; } = OpportunityStatus.New;

    public DateTime? ExpectedCloseDate { get; set; }

    // =========================================================
    // AMS Opportunity fields
    // =========================================================

    public string? AutoGenratedId { get; set; }

    public Guid? ContactPersonId { get; set; }

    [ForeignKey(nameof(ContactPersonId))]
    public AccountContacts? ContactPerson { get; set; }

    public DateTime ExpectedClosureDate { get; set; }

    public string? OpportunityName { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public Guid? ProposalId { get; set; }

    public bool IsClosed { get; set; } = false;

    public bool IsLost { get; set; } = false;

    public DateTime? IsLostDate { get; set; }

    public string? CompletedStatus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OpportunityAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OverAllDiscountPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NetAmount { get; set; }

    public bool IsRenewal { get; set; } = false;

    public bool IsReferral { get; set; } = false;

    public DateTime? ClosedDate { get; set; }

    public decimal? OpportunityAmountInDefaultCurrency { get; set; }

    public decimal? NetAmountInDefaultCurrency { get; set; }

    public decimal? ExchangeRateApplied { get; set; }

    public string? DefaultCurrencySymbol { get; set; }

    public string? ConvertCurrencySymbol { get; set; }

    public bool? IsExpansionOpportunity { get; set; }

    public bool? IsContractionOpportunity { get; set; }

    public bool? IsReactivation { get; set; }

    public string? ExpansionTypesSummary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TaxAmountInDefaultCurrency { get; set; }

    public bool IsDeleted { get; set; } = false;

    public Guid? PackageId { get; set; }

    public bool IsPackageModified { get; set; } = false;

    public bool HasCustomProducts { get; set; } = false;

    public bool? IsCreatedFromQbr { get; set; } = false;

    public Guid? SourceQbrId { get; set; }

    public string? AccountManagerId { get; set; }

    public string? AccountManagerSnapshotSource { get; set; }

    // =========================================================
    // Existing / AMS relationships
    // =========================================================

    public ICollection<Proposal> Proposals { get; set; } =
        new List<Proposal>();

    public ICollection<Order> Orders { get; set; } =
        new List<Order>();

    public ICollection<EngagementActivity> Activities { get; set; } =
        new List<EngagementActivity>();

    public ICollection<Conversation> Conversations { get; set; } =
        new List<Conversation>();
}

public class Proposal : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid OpportunityId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid CreatedByEmployeeId { get; set; }

    [Required, MaxLength(100)]
    public string ProposalNumber { get; set; } = string.Empty;

    public ProposalStatus Status { get; set; } = ProposalStatus.Draft;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public string? VoucherCode { get; set; }

    public decimal VoucherDiscountAmount { get; set; }

    public int Revision { get; set; } = 1;

    public string? CorrectionReason { get; set; }

    public DateTime? ValidUntil { get; set; }

    public Opportunity Opportunity { get; set; } = null!;

    public Customer Customer { get; set; } = null!;

    public Employee CreatedByEmployee { get; set; } = null!;

    public ICollection<ProposalProduct> ProposalProducts { get; set; } =
        new List<ProposalProduct>();

    public ICollection<Order> Orders { get; set; } =
        new List<Order>();

    public ICollection<Approval> Approvals { get; set; } =
        new List<Approval>();

    public ICollection<EngagementActivity> Activities { get; set; } =
        new List<EngagementActivity>();
}

public class ProposalProduct : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public DiscountType DiscountType { get; set; } =
        DiscountType.Percentage;

    public decimal DiscountValue { get; set; }

    public Proposal Proposal { get; set; } = null!;

    public Product Product { get; set; } = null!;
}

public class Order : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? AssignedEmployeeId { get; set; }

    [Required, MaxLength(100)]
    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    // =========================================================
    // AMS Order fields
    // =========================================================

    public string? AutoGenratedId { get; set; }

    public Guid? OpportunityId { get; set; }

    public Guid? ContactPersonId { get; set; }

    [ForeignKey(nameof(ContactPersonId))]
    public AccountContacts? ContactPerson { get; set; }

    public string? Description { get; set; }

    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

    public bool OrderStatusInBool { get; set; } = false;

    public string? AmsOrderStatus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OrderDiscountPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchaseAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NetAmount { get; set; }

    public bool IsOldPurchase { get; set; } = false;

    public decimal? OrderAmountInDefaultCurrency { get; set; }

    public decimal? NetAmountInDefaultCurrency { get; set; }

    public decimal? ExchangeRateApplied { get; set; }

    public string? DefaultCurrencySymbol { get; set; }

    public string? ConvertCurrencySymbol { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TaxAmountInDefaultCurrency { get; set; }

    public bool IsDeleted { get; set; } = false;

    public Guid? PackageId { get; set; }

    public bool IsPackageModified { get; set; } = false;

    public bool HasCustomProducts { get; set; } = false;

    public bool? IsExpansionOpportunity { get; set; }

    public bool? IsContractionOpportunity { get; set; }

    public bool? IsReactivation { get; set; }

    public string? ExpansionTypesSummary { get; set; }

    public string? AccountManagerId { get; set; }

    public string? AccountManagerSnapshotSource { get; set; }

    // =========================================================
    // Relationships
    // =========================================================

    public Proposal Proposal { get; set; } = null!;

    public Customer Customer { get; set; } = null!;

    public string? AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    public Account? Account { get; set; }

    public Employee? AssignedEmployee { get; set; }

    [ForeignKey(nameof(OpportunityId))]
    public Opportunity? Opportunity { get; set; }

    public ICollection<OrderProduct> OrderProducts { get; set; } =
        new List<OrderProduct>();
}

public class OrderProduct : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // =========================================================
    // AMS OrderProducts fields
    // =========================================================

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountPercentage { get; set; }

    public int? Term { get; set; }

    public decimal? DiscountedRate { get; set; }

    public DurationType DurationType { get; set; }

    public int ValidityDuration { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? RenewalDate { get; set; }

    public string? AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    public Account? Account { get; set; }

    public bool? OrderStatus { get; set; } = false;

    public decimal? ProductTotalAmountInDefaultCurrency { get; set; }

    public string? DefaultCurrencySymbol { get; set; }

    public string? ConvertCurrencySymbol { get; set; }

    public bool IsConvertedRenewalToOpportunity { get; set; } = false;

    public Guid? ConvertedOpportunityId { get; set; }

    [ForeignKey(nameof(ConvertedOpportunityId))]
    public Opportunity? ConvertedOpportunity { get; set; }

    public int? ExtendedGraceDays { get; set; } = 0;

    // =========================================================
    // Existing relationships
    // =========================================================

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
}

public class PlayBook : AuditableEntity
{
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Version { get; set; } = 1;

    public PlayBookStatus Status { get; set; } = PlayBookStatus.Draft;

    public TriggerType TriggerType { get; set; } = TriggerType.Event;

    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<PlayBookStep> Steps { get; set; } =
        new List<PlayBookStep>();

    public ICollection<WorkflowTransition> Transitions { get; set; } =
        new List<WorkflowTransition>();

    public ICollection<WorkflowExecution> Executions { get; set; } =
        new List<WorkflowExecution>();
}

public class PlayBookStep : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid PlayBookId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public StepType StepType { get; set; }

    public string? ConfigurationJson { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public bool IsStartStep { get; set; }

    public bool IsEndStep { get; set; }

    public PlayBook PlayBook { get; set; } = null!;

    public ICollection<WorkflowTransition> SourceTransitions { get; set; } =
        new List<WorkflowTransition>();

    public ICollection<WorkflowTransition> TargetTransitions { get; set; } =
        new List<WorkflowTransition>();

    public ICollection<Condition> Conditions { get; set; } =
        new List<Condition>();
}

public class WorkflowTransition : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid PlayBookId { get; set; }

    public Guid FromStepId { get; set; }

    public Guid ToStepId { get; set; }

    public Guid? ConditionId { get; set; }

    public string? Label { get; set; }

    public int Priority { get; set; }

    public PlayBook PlayBook { get; set; } = null!;

    public PlayBookStep FromStep { get; set; } = null!;

    public PlayBookStep ToStep { get; set; } = null!;

    public Condition? Condition { get; set; }
}

public class Condition : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid StepId { get; set; }

    [Required, MaxLength(200)]
    public string Field { get; set; } = string.Empty;

    public ConditionOperator Operator { get; set; }

    public string? Value { get; set; }

    public string DataType { get; set; } = "string";

    public PlayBookStep Step { get; set; } = null!;

    public ICollection<WorkflowTransition> WorkflowTransitions { get; set; } =
        new List<WorkflowTransition>();
}

public class WorkflowExecution : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid PlayBookId { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public Guid? CurrentStepId { get; set; }

    public WorkflowStatus Status { get; set; } =
        WorkflowStatus.Running;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public PlayBook PlayBook { get; set; } = null!;

    public ICollection<WorkflowExecutionStep> Steps { get; set; } =
        new List<WorkflowExecutionStep>();

    public ICollection<WorkflowHistory> Histories { get; set; } =
        new List<WorkflowHistory>();
}

public class WorkflowExecutionStep : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid WorkflowExecutionId { get; set; }

    public Guid PlayBookStepId { get; set; }

    public WorkflowStatus Status { get; set; } =
        WorkflowStatus.Running;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Result { get; set; }

    public string? ErrorMessage { get; set; }

    public WorkflowExecution WorkflowExecution { get; set; } = null!;
}

public class WorkflowHistory : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid WorkflowExecutionId { get; set; }

    public Guid? StepId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? PerformedBy { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Details { get; set; }

    public WorkflowExecution WorkflowExecution { get; set; } = null!;
}

public class Approval : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }

    public Guid? WorkflowExecutionId { get; set; }

    public Guid ApproverEmployeeId { get; set; }

    public int ProposalRevision { get; set; } = 1;

    public int ApprovalLevel { get; set; }

    public ApprovalStatus Status { get; set; } =
        ApprovalStatus.Pending;

    public string? Comments { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }

    public Proposal Proposal { get; set; } = null!;

    public Employee ApproverEmployee { get; set; } = null!;

    public WorkflowExecution? WorkflowExecution { get; set; }
}

public class ProposalRevision : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }

    public int Revision { get; set; }

    public string? CorrectionReason { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal VoucherDiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public Proposal Proposal { get; set; } = null!;
}

public class EngagementActivity : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? OpportunityId { get; set; }

    public Guid? ProposalId { get; set; }

    public Guid? SubscriptionId { get; set; }

    public Guid? RenewalReminderId { get; set; }

    [Required, MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string? Description { get; set; }

    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;

    public Employee? Employee { get; set; }

    public Opportunity? Opportunity { get; set; }

    public Proposal? Proposal { get; set; }

    public Subscription? Subscription { get; set; }

    public RenewalReminder? RenewalReminder { get; set; }
}

public class Conversation : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? OpportunityId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Channel { get; set; } = "Internal";

    public Customer Customer { get; set; } = null!;

    public Employee? Employee { get; set; }

    public Opportunity? Opportunity { get; set; }
}

public class Subscription : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid ProductId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public SubscriptionStatus Status { get; set; } =
        SubscriptionStatus.Active;

    public Customer Customer { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public ICollection<RenewalReminder> RenewalReminders { get; set; } =
        new List<RenewalReminder>();
}

public class RenewalReminder : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid SubscriptionId { get; set; }

    public int OffsetDays { get; set; }

    public DateTime ReminderDate { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Subscription Subscription { get; set; } = null!;

    public ICollection<EngagementActivity> Activities { get; set; } =
        new List<EngagementActivity>();
}

public class Voucher : AuditableEntity
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumAmount { get; set; }

    public bool Stackable { get; set; }
}
