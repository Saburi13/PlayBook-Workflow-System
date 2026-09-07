namespace PlayBook.Domain;

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

public enum DiscountType
{
    Percentage,
    FixedAmount
}