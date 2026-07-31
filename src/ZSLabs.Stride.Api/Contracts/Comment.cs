namespace ZSLabs.Stride.Api.Contracts;

public sealed record Comment(
    int Id,
    int? TaskId,
    int? SubtaskId,
    int AuthorId,
    string AuthorUsername,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);