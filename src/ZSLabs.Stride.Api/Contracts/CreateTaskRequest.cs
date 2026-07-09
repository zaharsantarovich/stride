namespace ZSLabs.Stride.Api.Contracts;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskStatus? Status,
    TaskPriority Priority,
    int? AssigneeId,
    DateTime? DueDate);