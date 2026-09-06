using Microsoft.AspNetCore.Mvc;
using PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.WorkflowResponseDTOs;
using PlayBook.Business.Interfaces.IService;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/workflows")]
public sealed class WorkflowController(
    IWorkflowService workflowService,
    IWorkflowExecutionService executionService) : ControllerBase
{
    [HttpGet("playbooks")]
    public async Task<ActionResult<IReadOnlyList<WorkflowPlayBookSummaryDto>>> GetPlayBooks(
        CancellationToken cancellationToken)
    {
        return Ok(
            await workflowService.GetPlayBooksAsync(
                cancellationToken));
    }

    [HttpGet("playbooks/{id:guid}")]
    public async Task<ActionResult<WorkflowPlayBookDto>> GetPlayBook(
        Guid id,
        CancellationToken cancellationToken)
    {
        var playBook =
            await workflowService.GetPlayBookAsync(
                id,
                cancellationToken);

        return playBook is null
            ? NotFound()
            : Ok(playBook);
    }

    [HttpPost("playbooks")]
    public async Task<ActionResult<WorkflowPlayBookSummaryDto>> CreatePlayBook(
        [FromBody] CreatePlayBookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var playBook =
                await workflowService.CreatePlayBookAsync(
                    request,
                    cancellationToken);

            return Ok(playBook);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("playbooks/{id:guid}")]
    public async Task<ActionResult<WorkflowPlayBookSummaryDto>> UpdatePlayBook(
        Guid id,
        [FromBody] CreatePlayBookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var playBook =
                await workflowService.UpdatePlayBookAsync(
                    id,
                    request,
                    cancellationToken);

            return playBook is null
                ? NotFound()
                : Ok(playBook);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("playbooks/{id:guid}/activate")]
    public async Task<ActionResult<WorkflowPlayBookSummaryDto>> ActivatePlayBook(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var playBook =
                await workflowService.ActivatePlayBookAsync(
                    id,
                    cancellationToken);

            return playBook is null
                ? NotFound()
                : Ok(playBook);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("executions")]
    public async Task<ActionResult<WorkflowExecutionDto>> Start(
        [FromBody] StartWorkflowApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var execution =
                await executionService.StartAsync(
                    new StartWorkflowRequest(
                        request.PlayBookId,
                        request.EntityType,
                        request.EntityId,
                        request.Payload),
                    cancellationToken);

            return Ok(execution);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("executions/{id:guid}/resume")]
    public async Task<ActionResult<WorkflowExecutionDto>> Resume(
        Guid id,
        [FromBody] ResumeWorkflowApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await executionService.ResumeAsync(
                    id,
                    request.Payload,
                    cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("executions/{id:guid}")]
    public async Task<ActionResult<WorkflowExecutionDto>> GetExecution(
        Guid id,
        CancellationToken cancellationToken)
    {
        var execution =
            await workflowService.GetExecutionAsync(
                id,
                cancellationToken);

        return execution is null
            ? NotFound()
            : Ok(execution);
    }
}