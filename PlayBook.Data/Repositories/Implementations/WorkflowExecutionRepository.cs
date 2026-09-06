using Microsoft.EntityFrameworkCore;
using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Implementations;

public sealed class WorkflowExecutionRepository(
    PlayBookDbContext dbContext) : IWorkflowExecutionRepository
{
    public async Task<IReadOnlyList<PlayBook.Domain.PlayBook>> GetActiveEventPlayBooksAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlayBooks
            .Where(playBook =>
                playBook.Status == PlayBookStatus.Active &&
                playBook.TriggerType == TriggerType.Event)
            .Include(playBook => playBook.Steps)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<PlayBook.Domain.PlayBook?> GetPlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlayBooks
            .Include(playBook => playBook.Steps)
                .ThenInclude(step => step.Conditions)
            .Include(playBook => playBook.Transitions)
                .ThenInclude(transition => transition.Condition)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                playBook => playBook.Id == id,
                cancellationToken);
    }

    public Task<Approval?> GetPendingApprovalAsync(
    Guid workflowExecutionId,
    Guid proposalId,
    CancellationToken cancellationToken = default)
    {
        return dbContext.Approvals
            .SingleOrDefaultAsync(
                approval =>
                    approval.WorkflowExecutionId == workflowExecutionId &&
                    approval.ProposalId == proposalId &&
                    approval.Status == ApprovalStatus.Pending,
                cancellationToken);
    }
    public async Task<WorkflowExecution?> GetWorkflowExecutionAsync(
        Guid executionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkflowExecutions
            .Include(execution => execution.PlayBook)
                .ThenInclude(playBook => playBook.Steps)
            .Include(execution => execution.PlayBook)
                .ThenInclude(playBook => playBook.Transitions)
                    .ThenInclude(transition => transition.Condition)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                execution => execution.Id == executionId,
                cancellationToken);
    }

    public async Task<Proposal?> GetProposalForWorkflowAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Proposals
            .Include(proposal => proposal.Opportunity)
            .Include(proposal => proposal.ProposalProducts)
                .ThenInclude(product => product.Product)
            .SingleOrDefaultAsync(
                proposal => proposal.Id == proposalId,
                cancellationToken);
    }

    public Task<Opportunity?> GetOpportunityAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Opportunities
            .SingleOrDefaultAsync(
                opportunity => opportunity.Id == opportunityId,
                cancellationToken);
    }

    public Task<Opportunity?> GetOpportunityForCustomerAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Opportunities
            .SingleOrDefaultAsync(
                opportunity => opportunity.Id == opportunityId,
                cancellationToken);
    }

    public Task<Proposal?> GetProposalForCustomerAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Proposals
            .SingleOrDefaultAsync(
                proposal => proposal.Id == proposalId,
                cancellationToken);
    }

    public Task<Employee?> GetEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Employees
            .Include(employee => employee.EmployeeGrade)
            .SingleOrDefaultAsync(
                employee => employee.Id == employeeId,
                cancellationToken);
    }

    public Task<Guid?> GetFirstActiveEmployeeIdAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.Employees
            .Where(employee => employee.IsActive)
            .Select(employee => (Guid?)employee.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> EmployeeExistsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Employees
            .AnyAsync(
                employee =>
                    employee.Id == employeeId &&
                    employee.IsActive,
                cancellationToken);
    }

    public Task<Product?> GetFirstActiveProductAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, Product>> GetActiveProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Where(product =>
                product.IsActive &&
                productIds.Contains(product.Id))
            .ToDictionaryAsync(
                product => product.Id,
                cancellationToken);
    }

    public Task<Order?> GetOrderByProposalIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .Include(order => order.OrderProducts)
            .SingleOrDefaultAsync(
                order => order.ProposalId == proposalId,
                cancellationToken);
    }

    public Task<Subscription?> GetLatestActiveSubscriptionAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Subscriptions
            .Where(subscription =>
                subscription.CustomerId == customerId &&
                subscription.ProductId == productId &&
                subscription.Status != SubscriptionStatus.Cancelled)
            .OrderByDescending(subscription => subscription.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetActiveOrExpiringSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Subscriptions
            .Include(subscription => subscription.Product)
            .Where(subscription =>
                subscription.Status == SubscriptionStatus.Active ||
                subscription.Status == SubscriptionStatus.Expiring)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> RenewalReminderExistsAsync(
        Guid subscriptionId,
        int offsetDays,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RenewalReminders
            .AnyAsync(reminder => reminder.SubscriptionId == subscriptionId && reminder.OffsetDays == offsetDays, cancellationToken);
    }

    public Task AddRenewalReminderAsync(
        RenewalReminder reminder,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RenewalReminders.AddAsync(reminder, cancellationToken).AsTask();
    }

    public Task<Voucher?> GetVoucherByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Vouchers
            .SingleOrDefaultAsync(
                voucher => voucher.Code == code,
                cancellationToken);
    }

    public Task<Guid?> GetCustomerIdFromOpportunityAsync(
        Guid opportunityId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Opportunities
            .Where(opportunity => opportunity.Id == opportunityId)
            .Select(opportunity => (Guid?)opportunity.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Guid?> GetCustomerIdFromProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Proposals
            .Where(proposal => proposal.Id == proposalId)
            .Select(proposal => (Guid?)proposal.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Guid?> GetCustomerIdFromSubscriptionAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Subscriptions
            .Where(subscription => subscription.Id == subscriptionId)
            .Select(subscription => (Guid?)subscription.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task AddExecutionAsync(
        WorkflowExecution execution,
        CancellationToken cancellationToken = default)
    {
        return dbContext.WorkflowExecutions
            .AddAsync(execution, cancellationToken)
            .AsTask();
    }

    public void AddExecutionStep(
        WorkflowExecutionStep executionStep)
    {
        dbContext.WorkflowExecutionSteps.Add(executionStep);
    }

    public void AddHistory(
        WorkflowHistory history)
    {
        dbContext.WorkflowHistories.Add(history);
    }

    public void AddOrder(
        Order order)
    {
        dbContext.Orders.Add(order);
    }

    public void AddSubscription(
        Subscription subscription)
    {
        dbContext.Subscriptions.Add(subscription);
    }

    public void AddProposal(
        Proposal proposal)
    {
        dbContext.Proposals.Add(proposal);
    }

    public void AddActivity(
        EngagementActivity activity)
    {
        dbContext.EngagementActivities.Add(activity);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}