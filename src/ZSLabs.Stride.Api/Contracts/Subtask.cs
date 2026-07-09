namespace ZSLabs.Stride.Api.Contracts;

public sealed record Subtask(
    int Id,
    int TaskId,
    string Title,
    string? Description,
    SubtaskStatus Status,
    int AuthorId,
    int? AssigneeId,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<Comment> Comments);