using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.Domain.Entities;

public class Subtask
{
    public Subtask()
    {
        Comments = new List<Comment>();
    }

    public Subtask(
        int taskId,
        string title,
        string? description,
        SubtaskStatus status,
        int authorId,
        int? assigneeId,
        DateTime? dueDateUtc,
        DateTime createdAtUtc)
        : this()
    {
        TaskId = taskId;
        Title = title;
        Description = description;
        Status = status;
        AuthorId = authorId;
        AssigneeId = assigneeId;
        DueDate = dueDateUtc;
        CreatedAt = createdAtUtc;
    }

    public int Id { get; set; }

    public int TaskId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public SubtaskStatus Status { get; set; }

    public int AuthorId { get; set; }

    public int? AssigneeId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Task? Task { get; set; }

    public User? Author { get; set; }

    public User? Assignee { get; set; }

    public ICollection<Comment> Comments { get; set; }
}