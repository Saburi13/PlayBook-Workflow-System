using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Interfaces;

public interface IApprovalRepository
{
    Task<Proposal?> GetProposalForApprovalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingApprovalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetApproverAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Approval>> GetForProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<Approval?> GetByIdAsync(
        Guid approvalId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Approval approval,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}