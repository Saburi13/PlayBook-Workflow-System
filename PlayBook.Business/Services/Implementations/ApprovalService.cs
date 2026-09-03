using PlayBook.Business.DTOs.Approval;
using PlayBook.Business.Services.Interfaces;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Business.Services.Implementations;

public sealed class ApprovalService(
    IApprovalRepository approvalRepository) : IApprovalService
{
    public async Task<ApprovalResponseDto> RequestAsync(
        Guid proposalId,
        Guid? workflowExecutionId = null,
        CancellationToken cancellationToken = default)
    {
        var proposal = await approvalRepository.GetProposalForApprovalAsync(
            proposalId,
            cancellationToken);

        if (proposal is null)
        {
            throw new KeyNotFoundException("Proposal was not found.");
        }

        if (proposal.Status is not (
            ProposalStatus.Draft or
            ProposalStatus.Rejected or
            ProposalStatus.CustomerRejected))
        {
            throw new InvalidOperationException(
                "Only draft, rejected, or customer-rejected proposals can be submitted for approval.");
        }

        var existingPending =
            await approvalRepository.HasPendingApprovalAsync(
                proposalId,
                cancellationToken);

        if (existingPending)
        {
            throw new InvalidOperationException(
                "The proposal already has a pending approval.");
        }

        var route = await FindApproverAsync(
            proposal.CreatedByEmployee,
            proposal.TotalAmount,
            cancellationToken);

        if (route is null)
        {
            throw new InvalidOperationException(
                "No active manager has sufficient approval authority for this proposal.");
        }

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
            ProposalRevision = proposal.Revision,
            Status = ApprovalStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        proposal.Status = ProposalStatus.PendingApproval;
        proposal.UpdatedAt = DateTime.UtcNow;

        await approvalRepository.AddAsync(
            approval,
            cancellationToken);

        await approvalRepository.SaveChangesAsync(
            cancellationToken);

        return ToDto(approval);
    }

    public Task<ApprovalResponseDto> ResubmitAsync(
        Guid proposalId,
        Guid? workflowExecutionId = null,
        CancellationToken cancellationToken = default)
    {
        return RequestAsync(
            proposalId,
            workflowExecutionId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalResponseDto>> GetForProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        var approvals =
            await approvalRepository.GetForProposalAsync(
                proposalId,
                cancellationToken);

        return approvals
            .Select(ToDto)
            .ToList();
    }

    public async Task<ApprovalResponseDto> DecideAsync(
        Guid approvalId,
        ApprovalDecisionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Decision is not (
            ApprovalStatus.Approved or
            ApprovalStatus.Rejected))
        {
            throw new ArgumentException(
                "Decision must be Approved or Rejected.");
        }

        var approval =
            await approvalRepository.GetByIdAsync(
                approvalId,
                cancellationToken);

        if (approval is null)
        {
            throw new KeyNotFoundException(
                "Approval was not found.");
        }

        if (approval.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException(
                "This approval has already been decided.");
        }

        if (approval.ApproverEmployeeId != request.ApproverEmployeeId)
        {
            throw new UnauthorizedAccessException(
                "Only the assigned approver can decide this approval.");
        }

        if (!approval.ApproverEmployee.IsActive)
        {
            throw new InvalidOperationException(
                "The assigned approver is inactive.");
        }

        approval.Status = request.Decision;

        approval.Comments =
            string.IsNullOrWhiteSpace(request.Comments)
                ? null
                : request.Comments.Trim();

        approval.RespondedAt = DateTime.UtcNow;
        approval.UpdatedAt = DateTime.UtcNow;

        approval.Proposal.Status =
            request.Decision == ApprovalStatus.Approved
                ? ProposalStatus.Approved
                : ProposalStatus.Rejected;

        approval.Proposal.UpdatedAt = DateTime.UtcNow;

        if (request.Decision == ApprovalStatus.Rejected &&
            approval.WorkflowExecution is not null)
        {
            approval.WorkflowExecution.Status =
                WorkflowStatus.Failed;

            approval.WorkflowExecution.ErrorMessage =
                "Approval rejected: " +
                (approval.Comments ?? "No reason provided.");

            approval.WorkflowExecution.CompletedAt =
                DateTime.UtcNow;
        }

        await approvalRepository.SaveChangesAsync(
            cancellationToken);

        return ToDto(approval);
    }

    public static IReadOnlyList<Employee> BuildApprovalHierarchy(
        Employee employee)
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

    private async Task<(Employee Employee, int Level)?> FindApproverAsync(
        Employee requester,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var managerId = requester.ManagerId;
        var level = 1;

        while (managerId.HasValue)
        {
            var manager =
                await approvalRepository.GetApproverAsync(
                    managerId.Value,
                    cancellationToken);

            if (manager is null)
            {
                return null;
            }

            if (manager.EmployeeGrade?.ApprovalLimit >= amount)
            {
                return (manager, level);
            }

            managerId = manager.ManagerId;
            level++;
        }

        return null;
    }

    private static ApprovalResponseDto ToDto(
        Approval approval)
    {
        return new ApprovalResponseDto(
            approval.Id,
            approval.ProposalId,
            approval.WorkflowExecutionId,
            approval.ApproverEmployeeId,
            approval.ApproverEmployee.FirstName +
                " " +
                approval.ApproverEmployee.LastName,
            approval.ApprovalLevel,
            approval.ProposalRevision,
            approval.Status,
            approval.Comments,
            approval.RequestedAt,
            approval.RespondedAt);
    }
}