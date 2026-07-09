namespace ZSLabs.Stride.Domain.Entities;

public class Comment
{
    public Comment()
    {
    }

    public Comment(int? taskId, int? subtaskId, int authorId, string content, DateTime createdAtUtc)
    {
        TaskId = taskId;
        SubtaskId = subtaskId;
        AuthorId = authorId;
        Content = content;
        CreatedAt = createdAtUtc;
    }

    public int Id { get; set; }

    public int? TaskId { get; set; }

    public int? SubtaskId { get; set; }

    public int AuthorId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Task? Task { get; set; }

    public Subtask? Subtask { get; set; }

    public User? Author { get; set; }
}