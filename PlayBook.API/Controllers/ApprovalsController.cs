using Microsoft.AspNetCore.Mvc;
using PlayBook.Application.Approvals;
using PlayBook.Application.Workflows;
using PlayBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using PlayBook.Domain;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/approvals")]
public sealed class ApprovalsController(IApprovalService approvalService, PlayBookDbContext dbContext, IWorkflowExecutionService workflowExecutionService) : ControllerBase
{
    [HttpPost("proposals/{proposalId:guid}")]
    public async Task<ActionResult<ApprovalDto>> RequestApproval(Guid proposalId, CancellationToken cancellationToken)
    {
        try { return Ok(await approvalService.RequestAsync(proposalId, cancellationToken: cancellationToken)); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
    }

    [HttpPost("proposals/{proposalId:guid}/resubmit")]
    public async Task<ActionResult<ApprovalDto>> ResubmitApproval(Guid proposalId, CancellationToken cancellationToken)
    {
        try
        {
            var previousExecution = await dbContext.WorkflowExecutions
                .Where(execution => execution.EntityType == "Proposal" && execution.EntityId == proposalId && execution.Status == WorkflowStatus.Failed)
                .OrderByDescending(execution => execution.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (previousExecution is not null)
            {
                var execution = await workflowExecutionService.StartAsync(new StartWorkflowRequest(previousExecution.PlayBookId, "Proposal", proposalId, new { forceManagerApproval = true }), cancellationToken);
                var approval = (await approvalService.GetForProposalAsync(proposalId, cancellationToken)).LastOrDefault(item => item.WorkflowExecutionId == execution.Id);
                if (approval is not null) return Ok(approval);
            }
            return Ok(await approvalService.ResubmitAsync(proposalId, cancellationToken: cancellationToken));
        }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
    }

    [HttpGet("proposals/{proposalId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ApprovalDto>>> GetForProposal(Guid proposalId, CancellationToken cancellationToken) =>
        Ok(await approvalService.GetForProposalAsync(proposalId, cancellationToken));

    [HttpPost("{approvalId:guid}/decision")]
    public async Task<ActionResult<ApprovalDto>> Decide(Guid approvalId, [FromBody] ApprovalDecisionRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await approvalService.DecideAsync(approvalId, request, cancellationToken)); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (UnauthorizedAccessException exception) { return Forbid(exception.Message); }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
    }
}
