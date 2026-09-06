using PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.WorkflowResponseDTOs;
using PlayBook.Business.Interfaces.IService;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Business.Implementations.Service;

public sealed class WorkflowService(
    IWorkflowRepository workflowRepository) : IWorkflowService
{
    public async Task<IReadOnlyList<WorkflowPlayBookSummaryDto>> GetPlayBooksAsync(
        CancellationToken cancellationToken = default)
    {
        var playBooks =
            await workflowRepository.GetPlayBooksAsync(cancellationToken);

        return playBooks
            .Select(ToSummaryDto)
            .ToList();
    }

    public async Task<WorkflowPlayBookDto?> GetPlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var playBook =
            await workflowRepository.GetPlayBookAsync(
                id,
                cancellationToken);

        return playBook is null
            ? null
            : ToDto(playBook);
    }

    public async Task<WorkflowPlayBookSummaryDto> CreatePlayBookAsync(
        CreatePlayBookRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "PlayBook name is required.",
                nameof(request));
        }

        if (request.Steps.Count == 0)
        {
            throw new InvalidOperationException(
                "A PlayBook requires at least one step.");
        }

        if (request.Steps.Count(step => step.IsStartStep) != 1)
        {
            throw new InvalidOperationException(
                "A PlayBook requires exactly one start step.");
        }

        var playBook = new PlayBook.Domain.PlayBook
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Status = request.Status,
            TriggerType = request.TriggerType,
            CreatedBy = request.CreatedBy.Trim()
        };

        var steps = request.Steps
            .Select(stepRequest =>
                new PlayBookStep
                {
                    Id = Guid.NewGuid(),
                    PlayBookId = playBook.Id,
                    Name = stepRequest.Name.Trim(),
                    Description = stepRequest.Description,
                    StepType = stepRequest.StepType,
                    ConfigurationJson = stepRequest.ConfigurationJson,
                    PositionX = stepRequest.PositionX,
                    PositionY = stepRequest.PositionY,
                    IsStartStep = stepRequest.IsStartStep,
                    IsEndStep = stepRequest.IsEndStep
                })
            .ToList();

        playBook.Steps = steps;

        var transitions = request.Transitions
            .Select(transitionRequest =>
            {
                var fromStepId =
                    GetStepId(
                        steps,
                        transitionRequest.FromStepIndex);

                var toStepId =
                    GetStepId(
                        steps,
                        transitionRequest.ToStepIndex);

                return new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    PlayBookId = playBook.Id,
                    FromStepId = fromStepId,
                    ToStepId = toStepId,
                    Label = transitionRequest.Label,
                    Priority = transitionRequest.Priority,
                    Condition = transitionRequest.Condition is null
                        ? null
                        : new Condition
                        {
                            Id = Guid.NewGuid(),
                            StepId = fromStepId,
                            Field = transitionRequest.Condition.Field,
                            Operator = transitionRequest.Condition.Operator,
                            Value = transitionRequest.Condition.Value,
                            DataType = transitionRequest.Condition.DataType
                        }
                };
            })
            .ToList();

        playBook.Transitions = transitions;

        await workflowRepository.AddPlayBookAsync(
            playBook,
            cancellationToken);

        await workflowRepository.SaveChangesAsync(
            cancellationToken);

        return ToSummaryDto(playBook);
    }

    public async Task<WorkflowPlayBookSummaryDto?> UpdatePlayBookAsync(
        Guid id,
        CreatePlayBookRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Steps.Count == 0)
        {
            throw new InvalidOperationException(
                "A PlayBook requires at least one step.");
        }

        if (request.Steps.Count(step => step.IsStartStep) != 1)
        {
            throw new InvalidOperationException(
                "A PlayBook requires exactly one start step.");
        }

        var playBook =
            await workflowRepository.GetPlayBookForUpdateAsync(
                id,
                cancellationToken);

        if (playBook is null)
        {
            return null;
        }

        playBook.Name = request.Name.Trim();
        playBook.Description = request.Description;
        playBook.Status = request.Status;
        playBook.TriggerType = request.TriggerType;
        playBook.CreatedBy = request.CreatedBy.Trim();
        playBook.Version++;

        var existingConditions = playBook.Steps
            .SelectMany(step => step.Conditions)
            .ToList();

        var existingTransitions =
            playBook.Transitions.ToList();

        var existingSteps =
            playBook.Steps.ToList();

        workflowRepository.RemoveConditions(
            existingConditions);

        workflowRepository.RemoveTransitions(
            existingTransitions);

        workflowRepository.RemoveSteps(
            existingSteps);

        var steps = request.Steps
            .Select(stepRequest =>
                new PlayBookStep
                {
                    Id = Guid.NewGuid(),
                    PlayBookId = playBook.Id,
                    Name = stepRequest.Name.Trim(),
                    Description = stepRequest.Description,
                    StepType = stepRequest.StepType,
                    ConfigurationJson = stepRequest.ConfigurationJson,
                    PositionX = stepRequest.PositionX,
                    PositionY = stepRequest.PositionY,
                    IsStartStep = stepRequest.IsStartStep,
                    IsEndStep = stepRequest.IsEndStep
                })
            .ToList();

        var transitions = request.Transitions
            .Select(transitionRequest =>
            {
                var fromStepId =
                    GetStepId(
                        steps,
                        transitionRequest.FromStepIndex);

                var toStepId =
                    GetStepId(
                        steps,
                        transitionRequest.ToStepIndex);

                return new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    PlayBookId = playBook.Id,
                    FromStepId = fromStepId,
                    ToStepId = toStepId,
                    Label = transitionRequest.Label,
                    Priority = transitionRequest.Priority,
                    Condition = transitionRequest.Condition is null
                        ? null
                        : new Condition
                        {
                            Id = Guid.NewGuid(),
                            StepId = fromStepId,
                            Field = transitionRequest.Condition.Field,
                            Operator = transitionRequest.Condition.Operator,
                            Value = transitionRequest.Condition.Value,
                            DataType = transitionRequest.Condition.DataType
                        }
                };
            })
            .ToList();

        workflowRepository.AddSteps(steps);
        workflowRepository.AddTransitions(transitions);

        await workflowRepository.SaveChangesAsync(
            cancellationToken);

        return ToSummaryDto(playBook);
    }

    public async Task<WorkflowPlayBookSummaryDto?> ActivatePlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var playBook =
            await workflowRepository.GetPlayBookForUpdateAsync(
                id,
                cancellationToken);

        if (playBook is null)
        {
            return null;
        }

        if (playBook.Steps.Count == 0 ||
            playBook.Steps.Count(step => step.IsStartStep) != 1 ||
            !playBook.Steps.Any(step => step.IsEndStep))
        {
            throw new InvalidOperationException(
                "An active PlayBook requires one start step and one end step.");
        }

        playBook.Status = PlayBookStatus.Active;

        await workflowRepository.SaveChangesAsync(
            cancellationToken);

        return ToSummaryDto(playBook);
    }

    public async Task<WorkflowExecutionDto?> GetExecutionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var execution =
            await workflowRepository.GetWorkflowExecutionAsync(
                id,
                cancellationToken);

        if (execution is null)
        {
            return null;
        }

        return new WorkflowExecutionDto(
            execution.Id,
            execution.PlayBookId,
            execution.EntityType,
            execution.EntityId,
            execution.CurrentStepId,
            execution.Status,
            execution.ErrorMessage);
    }

    private static Guid GetStepId(
        IReadOnlyList<PlayBookStep> steps,
        int index)
    {
        if (index < 0 || index >= steps.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                "Transition step index is invalid.");
        }

        return steps[index].Id;
    }

    private static WorkflowPlayBookSummaryDto ToSummaryDto(
        PlayBook.Domain.PlayBook playBook)
    {
        return new WorkflowPlayBookSummaryDto(
            playBook.Id,
            playBook.Name,
            playBook.Status,
            playBook.Version,
            playBook.TriggerType,
            playBook.CreatedBy);
    }

    private static WorkflowPlayBookDto ToDto(
        PlayBook.Domain.PlayBook playBook)
    {
        var steps = playBook.Steps
            .Select(step =>
                new WorkflowPlayBookStepDto(
                    step.Id,
                    step.Name,
                    step.Description,
                    step.StepType,
                    step.ConfigurationJson,
                    step.PositionX,
                    step.PositionY,
                    step.IsStartStep,
                    step.IsEndStep))
            .ToList();

        var transitions = playBook.Transitions
            .Select(transition =>
                new WorkflowTransitionDto(
                    transition.Id,
                    transition.FromStepId,
                    transition.ToStepId,
                    transition.Label,
                    transition.Priority,
                    transition.Condition is null
                        ? null
                        : new WorkflowConditionDto(
                            transition.Condition.Field,
                            transition.Condition.Operator,
                            transition.Condition.Value,
                            transition.Condition.DataType)))
            .ToList();

        return new WorkflowPlayBookDto(
            playBook.Id,
            playBook.Name,
            playBook.Description,
            playBook.Version,
            playBook.Status,
            playBook.TriggerType,
            playBook.CreatedBy,
            steps,
            transitions);
    }
}