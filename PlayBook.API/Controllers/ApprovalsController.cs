using Microsoft.AspNetCore.Mvc;
using PlayBook.Business.BusinessModels.RequestDTOs.ApprovalRequestDTOs;
using PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.ApprovalResponseDTOs;
using PlayBook.Business.Interfaces.IService;
using PlayBook.Data.Repositories.Interfaces;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/approvals")]
public sealed class ApprovalsController(
    IApprovalService approvalService,
    IApprovalRepository approvalRepository,
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
            var previousExecution =
                await approvalRepository.GetLastFailedWorkflowExecutionForProposalAsync(
                    proposalId,
                    cancellationToken);

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