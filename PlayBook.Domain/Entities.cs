using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayBook.Domain;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum EmployeeRole
{
    Admin,
    Manager,
    Employee,
    Approver
}

public enum ProposalStatus
{
    Draft,
    Submitted,
    PendingApproval,
    Approved,
    Rejected,
    CustomerPending,
    CustomerApproved,
    CustomerRejected,
    Expired
}

public enum OpportunityStatus
{
    New,
    InProgress,
    Proposal,
    Approval,
    CustomerApproval,
    Won,
    Lost,
    Closed
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Completed,
    Cancelled
}

public enum PlayBookStatus
{
    Draft,
    Active,
    Inactive,
    Archived
}

public enum TriggerType
{
    Event,
    Date,
    Manual,
    Condition
}

public enum StepType
{
    Trigger,
    Action,
    Condition,
    Approval,
    Notification,
    CustomerAction,
    EmployeeAssignment,
    Wait,
    End
}

public enum WorkflowStatus
{
    Running,
    Waiting,
    Completed,
    Failed,
    Cancelled
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum SubscriptionStatus
{
    Active,
    Expiring,
    Expired,
    Renewed,
    Cancelled
}

public enum ConditionOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull
}

public class EmployeeGrade : AuditableEntity
{
    public Guid Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ApprovalLimit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
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
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
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

    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<EngagementActivity> Activities { get; set; } = new List<EngagementActivity>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

public class Product : AuditableEntity
{
    public Guid Id { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProposalProduct> ProposalProducts { get; set; } = new List<ProposalProduct>();
    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

public class Opportunity : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.New;
    public DateTime? ExpectedCloseDate { get; set; }

    public Customer Customer { get; set; } = null!;
    public Employee? AssignedEmployee { get; set; }
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<EngagementActivity> Activities { get; set; } = new List<EngagementActivity>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
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
    public DateTime? ValidUntil { get; set; }

    public Opportunity Opportunity { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Employee CreatedByEmployee { get; set; } = null!;
    public ICollection<ProposalProduct> ProposalProducts { get; set; } = new List<ProposalProduct>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    public ICollection<EngagementActivity> Activities { get; set; } = new List<EngagementActivity>();
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
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public Proposal Proposal { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Employee? AssignedEmployee { get; set; }
    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
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

    public Order Order { get; set; } = null!;
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

    public ICollection<PlayBookStep> Steps { get; set; } = new List<PlayBookStep>();
    public ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
    public ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
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
    public ICollection<WorkflowTransition> SourceTransitions { get; set; } = new List<WorkflowTransition>();
    public ICollection<WorkflowTransition> TargetTransitions { get; set; } = new List<WorkflowTransition>();
    public ICollection<Condition> Conditions { get; set; } = new List<Condition>();
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
    public ICollection<WorkflowTransition> WorkflowTransitions { get; set; } = new List<WorkflowTransition>();
}

public class WorkflowExecution : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PlayBookId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid? CurrentStepId { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public PlayBook PlayBook { get; set; } = null!;
    public ICollection<WorkflowExecutionStep> Steps { get; set; } = new List<WorkflowExecutionStep>();
    public ICollection<WorkflowHistory> Histories { get; set; } = new List<WorkflowHistory>();
}

public class WorkflowExecutionStep : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowExecutionId { get; set; }
    public Guid PlayBookStepId { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;
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
    public int ApprovalLevel { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? Comments { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    public Proposal Proposal { get; set; } = null!;
    public Employee ApproverEmployee { get; set; } = null!;
}

public class EngagementActivity : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? ProposalId { get; set; }
    [Required, MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
    public Employee? Employee { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Proposal? Proposal { get; set; }
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
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public Customer Customer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
