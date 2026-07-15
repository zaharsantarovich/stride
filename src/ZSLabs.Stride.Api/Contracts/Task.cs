namespace ZSLabs.Stride.Api.Contracts;

public sealed record Task(
    int Id,
    int SpaceId,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    int AuthorId,
    int? AssigneeId,
    string? AssigneeUsername,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<Subtask> Subtasks,
    IReadOnlyList<Comment> Comments);