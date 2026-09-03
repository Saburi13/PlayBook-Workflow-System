using System;
using System.Collections.Generic;
using System.Text;

namespace PlayBook.Business.DTOs.Workflow;

public sealed record StartWorkflowRequest(
    Guid PlayBookId,
    string EntityType,
    Guid EntityId,
    object? Payload);