using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.Domain.Entities;

public class Task
{
    public Task()
    {
        Subtasks = new List<Subtask>();
        Comments = new List<Comment>();
    }

    public Task(
        int spaceId,
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        int authorId,
        int? assigneeId,
        DateTime? dueDateUtc,
        DateTime createdAtUtc)
        : this()
    {
        SpaceId = spaceId;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        AuthorId = authorId;
        AssigneeId = assigneeId;
        DueDate = dueDateUtc;
        CreatedAt = createdAtUtc;
    }

    public int Id { get; set; }

    public int SpaceId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public int AuthorId { get; set; }

    public int? AssigneeId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Space? Space { get; set; }

    public User? Author { get; set; }

    public User? Assignee { get; set; }

    public ICollection<Subtask> Subtasks { get; set; }

    public ICollection<Comment> Comments { get; set; }
}