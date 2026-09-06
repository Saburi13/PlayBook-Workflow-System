using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Interfaces;

public interface IWorkflowExecutionRepository
{
    Task<IReadOnlyList<PlayBook.Domain.PlayBook>> GetActiveEventPlayBooksAsync(
        CancellationToken cancellationToken = default);

    Task<PlayBook.Domain.PlayBook?> GetPlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WorkflowExecution?> GetWorkflowExecutionAsync(
        Guid executionId,
        CancellationToken cancellationToken = default);

    Task<Proposal?> GetProposalForWorkflowAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Opportunity?> GetOpportunityAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default);

    Task<Opportunity?> GetOpportunityForCustomerAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default);

    Task<Proposal?> GetProposalForCustomerAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetFirstActiveEmployeeIdAsync(
        CancellationToken cancellationToken = default);

    Task<bool> EmployeeExistsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<Product?> GetFirstActiveProductAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, Product>> GetActiveProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task<Order?> GetOrderByProposalIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Subscription?> GetLatestActiveSubscriptionAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetActiveOrExpiringSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> RenewalReminderExistsAsync(
        Guid subscriptionId,
        int offsetDays,
        CancellationToken cancellationToken = default);

    Task AddRenewalReminderAsync(
        RenewalReminder reminder,
        CancellationToken cancellationToken = default);

    Task<Voucher?> GetVoucherByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetCustomerIdFromOpportunityAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetCustomerIdFromProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Approval?> GetPendingApprovalAsync(
    Guid workflowExecutionId,
    Guid proposalId,
    CancellationToken cancellationToken = default);

    Task<Guid?> GetCustomerIdFromSubscriptionAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default);

    Task AddExecutionAsync(
        WorkflowExecution execution,
        CancellationToken cancellationToken = default);


    void AddExecutionStep(
        WorkflowExecutionStep executionStep);

    void AddHistory(
        WorkflowHistory history);

    void AddOrder(
        Order order);

    void AddSubscription(
        Subscription subscription);

    void AddProposal(
        Proposal proposal);

    void AddActivity(
        EngagementActivity activity);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}