using System.Text.Json;
using PlayBook.Domain;

namespace PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;

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