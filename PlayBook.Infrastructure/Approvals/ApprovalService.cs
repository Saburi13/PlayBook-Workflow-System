using Microsoft.EntityFrameworkCore;
using PlayBook.Application.Approvals;
using PlayBook.Domain;
using PlayBook.Infrastructure.Data;

namespace PlayBook.Infrastructure.Approvals;

public sealed class ApprovalService(PlayBookDbContext dbContext) : IApprovalService
{
    public async Task<ApprovalDto> RequestAsync(Guid proposalId, Guid? workflowExecutionId = null, CancellationToken cancellationToken = default)
    {
        var proposal = await dbContext.Proposals
            .Include(item => item.CreatedByEmployee)
                .ThenInclude(employee => employee.EmployeeGrade)
            .SingleOrDefaultAsync(item => item.Id == proposalId, cancellationToken);

        if (proposal is null) throw new KeyNotFoundException("Proposal was not found.");
        if (proposal.Status is not (ProposalStatus.Draft or ProposalStatus.Rejected or ProposalStatus.CustomerRejected)) throw new InvalidOperationException("Only draft, rejected, or customer-rejected proposals can be submitted for approval.");

        var existingPending = await dbContext.Approvals.AnyAsync(item => item.ProposalId == proposalId && item.Status == ApprovalStatus.Pending, cancellationToken);
        if (existingPending) throw new InvalidOperationException("The proposal already has a pending approval.");

        var route = await FindApproverAsync(proposal.CreatedByEmployee, proposal.TotalAmount, cancellationToken);
        if (route is null) throw new InvalidOperationException("No active manager has sufficient approval authority for this proposal.");
        var (approver, approvalLevel) = route.Value;

        var approval = new Approval
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            WorkflowExecutionId = workflowExecutionId,
            Proposal = proposal,
            ApproverEmployeeId = approver.Id,
            ApproverEmployee = approver,
            ApprovalLevel = approvalLevel,
            Status = ApprovalStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };
        proposal.Status = ProposalStatus.PendingApproval;
        proposal.UpdatedAt = DateTime.UtcNow;
        dbContext.Approvals.Add(approval);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(approval);
    }

    public Task<ApprovalDto> ResubmitAsync(Guid proposalId, Guid? workflowExecutionId = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync(proposalId, workflowExecutionId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalDto>> GetForProposalAsync(Guid proposalId, CancellationToken cancellationToken = default) =>
        await dbContext.Approvals.AsNoTracking()
            .Include(item => item.ApproverEmployee)
            .Where(item => item.ProposalId == proposalId)
            .OrderBy(item => item.ApprovalLevel)
            .ThenBy(item => item.RequestedAt)
            .Select(item => new ApprovalDto(item.Id, item.ProposalId, item.WorkflowExecutionId, item.ApproverEmployeeId, item.ApproverEmployee.FirstName + " " + item.ApproverEmployee.LastName, item.ApprovalLevel, item.Status, item.Comments, item.RequestedAt, item.RespondedAt))
            .ToListAsync(cancellationToken);

    public async Task<ApprovalDto> DecideAsync(Guid approvalId, ApprovalDecisionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Decision is not (ApprovalStatus.Approved or ApprovalStatus.Rejected)) throw new ArgumentException("Decision must be Approved or Rejected.");

        var approval = await dbContext.Approvals
            .Include(item => item.Proposal)
            .Include(item => item.ApproverEmployee)
                .ThenInclude(employee => employee.EmployeeGrade)
            .SingleOrDefaultAsync(item => item.Id == approvalId, cancellationToken);

        if (approval is null) throw new KeyNotFoundException("Approval was not found.");
        if (approval.Status != ApprovalStatus.Pending) throw new InvalidOperationException("This approval has already been decided.");
        if (approval.ApproverEmployeeId != request.ApproverEmployeeId) throw new UnauthorizedAccessException("Only the assigned approver can decide this approval.");
        if (!approval.ApproverEmployee.IsActive) throw new InvalidOperationException("The assigned approver is inactive.");

        approval.Status = request.Decision;
        approval.Comments = string.IsNullOrWhiteSpace(request.Comments) ? null : request.Comments.Trim();
        approval.RespondedAt = DateTime.UtcNow;
        approval.UpdatedAt = DateTime.UtcNow;
        approval.Proposal.Status = request.Decision == ApprovalStatus.Approved ? ProposalStatus.Approved : ProposalStatus.Rejected;
        approval.Proposal.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(approval);
    }

    public static IReadOnlyList<Employee> BuildApprovalHierarchy(Employee employee)
    {
        var chain = new List<Employee>();
        var current = employee;

        while (current is not null)
        {
            chain.Add(current);
            current = current.Manager;
        }

        chain.Reverse();
        return chain;
    }

    private async Task<(Employee Employee, int Level)?> FindApproverAsync(Employee requester, decimal amount, CancellationToken cancellationToken)
    {
        var managerId = requester.ManagerId;
        var level = 1;
        while (managerId.HasValue)
        {
            var manager = await dbContext.Employees.Include(item => item.EmployeeGrade).SingleOrDefaultAsync(item => item.Id == managerId && item.IsActive, cancellationToken);
            if (manager is null) return null;
            if (manager.EmployeeGrade?.ApprovalLimit >= amount) return (manager, level);
            managerId = manager.ManagerId;
            level++;
        }
        return null;
    }

    private static ApprovalDto ToDto(Approval approval) =>
        new(approval.Id, approval.ProposalId, approval.WorkflowExecutionId, approval.ApproverEmployeeId, approval.ApproverEmployee.FirstName + " " + approval.ApproverEmployee.LastName, approval.ApprovalLevel, approval.Status, approval.Comments, approval.RequestedAt, approval.RespondedAt);
}
