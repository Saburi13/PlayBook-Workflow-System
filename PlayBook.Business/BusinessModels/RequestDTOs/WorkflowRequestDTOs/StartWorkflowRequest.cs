using System;
using System.Collections.Generic;
using System.Text;

namespace PlayBook.Business.BusinessModels.RequestDTOs.WorkflowRequestDTOs;

public sealed record StartWorkflowRequest(
    Guid PlayBookId,
    string EntityType,
    Guid EntityId,
    object? Payload);