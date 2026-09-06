using Microsoft.EntityFrameworkCore;
using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Implementations;

public sealed class CrmDataRepository(
    PlayBookDbContext dbContext) : ICrmDataRepository
{
    public async Task<Proposal?> GetProposalWithProductsAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Proposals
            .Include(proposal => proposal.ProposalProducts)
            .SingleOrDefaultAsync(
                proposal => proposal.Id == proposalId,
                cancellationToken);
    }

    public async Task<Proposal?> GetProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Proposals
            .SingleOrDefaultAsync(
                proposal => proposal.Id == proposalId,
                cancellationToken);
    }

    public Task AddProposalRevisionAsync(
        ProposalRevision revision,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProposalRevisions
            .AddAsync(revision, cancellationToken)
            .AsTask();
    }

    public async Task<IReadOnlyList<Voucher>> GetVouchersAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Vouchers
            .AsNoTracking()
            .OrderBy(voucher => voucher.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Voucher?> GetVoucherByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Vouchers
            .SingleOrDefaultAsync(
                voucher => voucher.Code == code,
                cancellationToken);
    }

    public Task<bool> VoucherCodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Vouchers
            .AnyAsync(
                voucher => voucher.Code == code,
                cancellationToken);
    }

    public Task AddVoucherAsync(
        Voucher voucher,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Vouchers
            .AddAsync(voucher, cancellationToken)
            .AsTask();
    }

    public async Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Subscriptions
            .AsNoTracking()
            .OrderByDescending(subscription => subscription.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EngagementActivity>> GetActivitiesAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EngagementActivities
            .AsNoTracking()
            .Where(activity =>
                (!customerId.HasValue ||
                 activity.CustomerId == customerId) &&
                (!opportunityId.HasValue ||
                 activity.OpportunityId == opportunityId))
            .OrderByDescending(activity => activity.ActivityDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddActivityAsync(
        EngagementActivity activity,
        CancellationToken cancellationToken = default)
    {
        return dbContext.EngagementActivities
            .AddAsync(activity, cancellationToken)
            .AsTask();
    }

    public async Task<IReadOnlyList<Conversation>> GetConversationsAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(conversation =>
                (!customerId.HasValue ||
                 conversation.CustomerId == customerId) &&
                (!opportunityId.HasValue ||
                 conversation.OpportunityId == opportunityId))
            .OrderByDescending(conversation => conversation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Conversations
            .AddAsync(conversation, cancellationToken)
            .AsTask();
    }

    public Task<bool> EmployeeGradeExistsAsync(
        Guid gradeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.EmployeeGrades
            .AnyAsync(
                grade => grade.Id == gradeId,
                cancellationToken);
    }

    public Task<bool> EmployeeExistsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Employees
            .AnyAsync(
                employee => employee.Id == employeeId,
                cancellationToken);
    }

    public Task<bool> CustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .AnyAsync(
                customer => customer.Id == customerId,
                cancellationToken);
    }

    public Task<bool> CustomerExistsForOpportunityAsync(
        Guid opportunityId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Opportunities
            .AnyAsync(
                opportunity =>
                    opportunity.Id == opportunityId &&
                    opportunity.CustomerId == customerId,
                cancellationToken);
    }

    public Task<bool> OpportunityExistsAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Opportunities
            .AnyAsync(
                opportunity => opportunity.Id == opportunityId,
                cancellationToken);
    }

    public Task<bool> ProductExistsAsync(
        Guid productId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        return activeOnly
            ? dbContext.Products.AnyAsync(
                product => product.Id == productId &&
                            product.IsActive,
                cancellationToken)
            : dbContext.Products.AnyAsync(
                product => product.Id == productId,
                cancellationToken);
    }

    public Task<bool> ProposalExistsAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Proposals
            .AnyAsync(
                proposal => proposal.Id == proposalId,
                cancellationToken);
    }

    public Task<bool> ProposalExistsForCustomerAsync(
        Guid proposalId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Proposals
            .AnyAsync(
                proposal =>
                    proposal.Id == proposalId &&
                    proposal.CustomerId == customerId,
                cancellationToken);
    }

    public Task<bool> OrderExistsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .AnyAsync(
                order => order.Id == orderId,
                cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetActiveProductPricesAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.IsActive &&
                productIds.Contains(product.Id))
            .ToDictionaryAsync(
                product => product.Id,
                product => product.Price,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}