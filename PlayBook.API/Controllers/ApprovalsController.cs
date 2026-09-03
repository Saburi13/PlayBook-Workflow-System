using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBook.Business.DTOs.Approval;
using PlayBook.Business.Services.Interfaces;
using PlayBook.Data.Context;
using PlayBook.Domain;
using PlayBook.Business.DTOs.Workflow;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/approvals")]
public sealed class ApprovalsController(
    IApprovalService approvalService,
    PlayBookDbContext dbContext,
    IWorkflowExecutionService workflowExecutionService) : ControllerBase
{
    [HttpPost("proposals/{proposalId:guid}")]
    public async Task<ActionResult<ApprovalResponseDto>> RequestApproval(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await approvalService.RequestAsync(
                    proposalId,
                    cancellationToken: cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPost("proposals/{proposalId:guid}/resubmit")]
    public async Task<ActionResult<ApprovalResponseDto>> ResubmitApproval(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        try
        {
            var previousExecution = await dbContext.WorkflowExecutions
                .Where(execution =>
                    execution.EntityType == "Proposal" &&
                    execution.EntityId == proposalId &&
                    execution.Status == WorkflowStatus.Failed)
                .OrderByDescending(execution => execution.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (previousExecution is not null)
            {
                var execution = await workflowExecutionService.StartAsync(
                    new StartWorkflowRequest(
                        previousExecution.PlayBookId,
                        "Proposal",
                        proposalId,
                        new { forceManagerApproval = true }),
                    cancellationToken);

                var approval = (
                    await approvalService.GetForProposalAsync(
                        proposalId,
                        cancellationToken))
                    .LastOrDefault(
                        item => item.WorkflowExecutionId == execution.Id);

                if (approval is not null)
                {
                    return Ok(approval);
                }
            }

            return Ok(
                await approvalService.ResubmitAsync(
                    proposalId,
                    cancellationToken: cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpGet("proposals/{proposalId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ApprovalResponseDto>>> GetForProposal(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await approvalService.GetForProposalAsync(
                proposalId,
                cancellationToken));
    }

    [HttpPost("{approvalId:guid}/decision")]
    public async Task<ActionResult<ApprovalResponseDto>> Decide(
        Guid approvalId,
        [FromBody] ApprovalDecisionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await approvalService.DecideAsync(
                    approvalId,
                    request,
                    cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Forbid(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }
}