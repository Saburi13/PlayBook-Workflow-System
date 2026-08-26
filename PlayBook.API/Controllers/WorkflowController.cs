using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBook.Application.Workflows;
using PlayBook.Domain;
using PlayBook.Infrastructure.Data;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/workflows")]
public sealed class WorkflowController(
    PlayBookDbContext dbContext,
    IWorkflowExecutionService executionService) : ControllerBase
{
    [HttpGet("playbooks")]
    public async Task<ActionResult<IEnumerable<object>>> GetPlayBooks(CancellationToken cancellationToken) =>
        Ok(await dbContext.PlayBooks.AsNoTracking().OrderBy(p => p.Name).Select(p => new { p.Id, p.Name, p.Version, p.Status, p.TriggerType, p.CreatedBy }).ToListAsync(cancellationToken));

    [HttpPost("playbooks")]
    public async Task<ActionResult<object>> CreatePlayBook(CreatePlayBookRequest request, CancellationToken cancellationToken)
    {
        if (request.Steps.Count == 0 || request.Steps.Count(step => step.IsStartStep) != 1) return BadRequest("A PlayBook requires exactly one start step.");
        var playBook = new PlayBook.Domain.PlayBook { Id = Guid.NewGuid(), Name = request.Name.Trim(), Description = request.Description, Status = request.Status, TriggerType = request.TriggerType, CreatedBy = request.CreatedBy.Trim(), Version = 1 };
        var steps = request.Steps.Select(step => new PlayBookStep { Id = Guid.NewGuid(), PlayBookId = playBook.Id, Name = step.Name.Trim(), Description = step.Description, StepType = step.StepType, ConfigurationJson = step.ConfigurationJson, PositionX = step.PositionX, PositionY = step.PositionY, IsStartStep = step.IsStartStep, IsEndStep = step.IsEndStep }).ToList();
        playBook.Steps = steps;
        playBook.Transitions = request.Transitions.Select(transition => new WorkflowTransition { Id = Guid.NewGuid(), PlayBookId = playBook.Id, FromStepId = GetStepId(steps, transition.FromStepIndex), ToStepId = GetStepId(steps, transition.ToStepIndex), Label = transition.Label, Priority = transition.Priority, Condition = transition.Condition is null ? null : new Condition { Id = Guid.NewGuid(), Field = transition.Condition.Field, Operator = transition.Condition.Operator, Value = transition.Condition.Value, DataType = transition.Condition.DataType, StepId = GetStepId(steps, transition.FromStepIndex) } }).ToList();
        dbContext.PlayBooks.Add(playBook);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetPlayBook), new { id = playBook.Id }, new { playBook.Id, playBook.Name, playBook.Status, playBook.Version });
    }

    [HttpGet("playbooks/{id:guid}")]
    public async Task<ActionResult<object>> GetPlayBook(Guid id, CancellationToken cancellationToken)
    {
        var playBook = await dbContext.PlayBooks.AsNoTracking().Include(p => p.Steps).Include(p => p.Transitions).ThenInclude(t => t.Condition).AsSplitQuery().SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (playBook is null) return NotFound();

        return Ok(new
        {
            playBook.Id,
            playBook.Name,
            playBook.Description,
            playBook.Version,
            playBook.Status,
            playBook.TriggerType,
            playBook.CreatedBy,
            Steps = playBook.Steps.Select(step => new
            {
                step.Id,
                step.Name,
                step.Description,
                step.StepType,
                step.ConfigurationJson,
                step.PositionX,
                step.PositionY,
                step.IsStartStep,
                step.IsEndStep
            }),
            Transitions = playBook.Transitions.Select(transition => new
            {
                transition.Id,
                transition.FromStepId,
                transition.ToStepId,
                transition.Label,
                transition.Priority,
                Condition = transition.Condition is null ? null : new
                {
                    transition.Condition.Field,
                    transition.Condition.Operator,
                    transition.Condition.Value,
                    transition.Condition.DataType
                }
            })
        });
    }

    [HttpPost("executions")]
    public async Task<ActionResult<WorkflowExecutionDto>> Start(StartWorkflowApiRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await executionService.StartAsync(new StartWorkflowRequest(request.PlayBookId, request.EntityType, request.EntityId, request.Payload), cancellationToken)); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return BadRequest(exception.Message); }
    }

    [HttpPost("executions/{id:guid}/resume")]
    public async Task<ActionResult<WorkflowExecutionDto>> Resume(Guid id, ResumeWorkflowApiRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await executionService.ResumeAsync(id, request.Payload, cancellationToken)); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return BadRequest(exception.Message); }
    }

    [HttpGet("executions/{id:guid}")]
    public async Task<ActionResult<WorkflowExecutionDto>> GetExecution(Guid id, CancellationToken cancellationToken)
    {
        var execution = await dbContext.WorkflowExecutions.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return execution is null ? NotFound() : Ok(new WorkflowExecutionDto(execution.Id, execution.PlayBookId, execution.EntityType, execution.EntityId, execution.CurrentStepId, execution.Status, execution.ErrorMessage));
    }

    private static Guid GetStepId(IReadOnlyList<PlayBookStep> steps, int index) =>
        index >= 0 && index < steps.Count ? steps[index].Id : throw new ArgumentOutOfRangeException(nameof(index), "Transition step index is invalid.");
}

public sealed class CreatePlayBookRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlayBookStatus Status { get; set; } = PlayBookStatus.Draft;
    public TriggerType TriggerType { get; set; } = TriggerType.Manual;
    public string CreatedBy { get; set; } = string.Empty;
    public List<CreatePlayBookStepRequest> Steps { get; set; } = [];
    public List<CreateWorkflowTransitionRequest> Transitions { get; set; } = [];
}

public sealed class CreatePlayBookStepRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StepType StepType { get; set; }
    public string? ConfigurationJson { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public bool IsStartStep { get; set; }
    public bool IsEndStep { get; set; }
}

public sealed class CreateWorkflowTransitionRequest
{
    public int FromStepIndex { get; set; }
    public int ToStepIndex { get; set; }
    public string? Label { get; set; }
    public int Priority { get; set; }
    public CreateConditionRequest? Condition { get; set; }
}

public sealed class CreateConditionRequest
{
    public string Field { get; set; } = string.Empty;
    public ConditionOperator Operator { get; set; }
    public string? Value { get; set; }
    public string DataType { get; set; } = "string";
}

public sealed class StartWorkflowApiRequest
{
    public Guid PlayBookId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public JsonElement? Payload { get; set; }
}

public sealed class ResumeWorkflowApiRequest
{
    public JsonElement? Payload { get; set; }
}
