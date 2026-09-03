using PlayBook.Domain;

namespace PlayBook.Business.DTOs.Workflow;

public sealed record WorkflowExecutionDto(
    Guid Id,
    Guid PlayBookId,
    string EntityType,
    Guid EntityId,
    Guid? CurrentStepId,
    WorkflowStatus Status,
    string? ErrorMessage);