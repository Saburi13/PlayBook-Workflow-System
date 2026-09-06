using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Interfaces;

public interface ICrmDataRepository
{
    Task<Proposal?> GetProposalWithProductsAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Proposal?> GetProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task AddProposalRevisionAsync(
        ProposalRevision revision,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Voucher>> GetVouchersAsync(
        CancellationToken cancellationToken = default);

    Task<Voucher?> GetVoucherByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> VoucherCodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task AddVoucherAsync(
        Voucher voucher,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EngagementActivity>> GetActivitiesAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default);

    Task AddActivityAsync(
        EngagementActivity activity,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> GetConversationsAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default);

    Task AddConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    Task<bool> EmployeeGradeExistsAsync(
        Guid gradeId,
        CancellationToken cancellationToken = default);

    Task<bool> EmployeeExistsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<bool> CustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<bool> CustomerExistsForOpportunityAsync(
        Guid opportunityId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<bool> OpportunityExistsAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default);

    Task<bool> ProductExistsAsync(
        Guid productId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    Task<bool> ProposalExistsAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<bool> ProposalExistsForCustomerAsync(
        Guid proposalId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<bool> OrderExistsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, decimal>> GetActiveProductPricesAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}