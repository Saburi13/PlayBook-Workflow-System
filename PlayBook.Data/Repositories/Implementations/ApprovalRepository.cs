using Microsoft.EntityFrameworkCore;
using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Implementations;

public sealed class ApprovalRepository(
    PlayBookDbContext dbContext) : IApprovalRepository
{
    public async Task<Proposal?> GetProposalForApprovalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Proposals
            .Include(item => item.CreatedByEmployee)
                .ThenInclude(employee => employee.EmployeeGrade)
            .SingleOrDefaultAsync(
                item => item.Id == proposalId,
                cancellationToken);
    }

    public Task<bool> HasPendingApprovalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Approvals.AnyAsync(
            item =>
                item.ProposalId == proposalId &&
                item.Status == ApprovalStatus.Pending,
            cancellationToken);
    }

    public async Task<Employee?> GetApproverAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .Include(item => item.EmployeeGrade)
            .SingleOrDefaultAsync(
                item =>
                    item.Id == employeeId &&
                    item.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Approval>> GetForProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Approvals
            .AsNoTracking()
            .Include(item => item.ApproverEmployee)
            .Where(item => item.ProposalId == proposalId)
            .OrderBy(item => item.ApprovalLevel)
            .ThenBy(item => item.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Approval?> GetByIdAsync(
        Guid approvalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Approvals
            .Include(item => item.Proposal)
            .Include(item => item.WorkflowExecution)
            .Include(item => item.ApproverEmployee)
                .ThenInclude(employee => employee.EmployeeGrade)
            .SingleOrDefaultAsync(
                item => item.Id == approvalId,
                cancellationToken);
    }

    public Task AddAsync(
        Approval approval,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Approvals
            .AddAsync(approval, cancellationToken)
            .AsTask();
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}