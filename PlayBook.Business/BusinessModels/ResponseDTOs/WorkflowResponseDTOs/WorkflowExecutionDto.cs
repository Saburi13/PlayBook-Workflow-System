using PlayBook.Domain;

namespace PlayBook.Business.BusinessModels.ResponseDTOs.WorkflowResponseDTOs;

public sealed record WorkflowExecutionDto(
    Guid Id,
    Guid PlayBookId,
    string EntityType,
    Guid EntityId,
    Guid? CurrentStepId,
    WorkflowStatus Status,
    string? ErrorMessage);