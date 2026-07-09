namespace ZSLabs.Stride.Api.Contracts;

public sealed record UpdateTaskRequest(
    string? Title,
    string? Description,
    TaskStatus? Status,
    TaskPriority? Priority,
    int? AssigneeId,
    DateTime? DueDate);