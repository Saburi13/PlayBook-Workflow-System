using PlayBook.Domain;

namespace PlayBook.Business.BusinessModels.ResponseDTOs.WorkflowResponseDTOs;

public sealed record WorkflowPlayBookSummaryDto(
    Guid Id,
    string Name,
    PlayBookStatus Status,
    int Version,
    TriggerType TriggerType,
    string CreatedBy);

public sealed record WorkflowPlayBookStepDto(
    Guid Id,
    string Name,
    string? Description,
    StepType StepType,
    string? ConfigurationJson,
    double PositionX,
    double PositionY,
    bool IsStartStep,
    bool IsEndStep);

public sealed record WorkflowConditionDto(
    string Field,
    ConditionOperator Operator,
    string? Value,
    string DataType);

public sealed record WorkflowTransitionDto(
    Guid Id,
    Guid FromStepId,
    Guid ToStepId,
    string? Label,
    int Priority,
    WorkflowConditionDto? Condition);

public sealed record WorkflowPlayBookDto(
    Guid Id,
    string Name,
    string? Description,
    int Version,
    PlayBookStatus Status,
    TriggerType TriggerType,
    string CreatedBy,
    IReadOnlyList<WorkflowPlayBookStepDto> Steps,
    IReadOnlyList<WorkflowTransitionDto> Transitions);