using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.Domain.Entities;

public class User
{
    public User()
    {
        AuthoredSpaces = new List<Space>();
        AuthoredTasks = new List<Task>();
        AssignedTasks = new List<Task>();
        AuthoredSubtasks = new List<Subtask>();
        AssignedSubtasks = new List<Subtask>();
        Comments = new List<Comment>();
    }

    public User(string username, string passwordHash, string? email, UserRole role, DateTime createdAtUtc)
        : this()
    {
        Username = username;
        PasswordHash = passwordHash;
        Email = email;
        Role = role;
        CreatedAt = createdAtUtc;
    }

    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? Email { get; set; }

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Space> AuthoredSpaces { get; set; }

    public ICollection<Task> AuthoredTasks { get; set; }

    public ICollection<Task> AssignedTasks { get; set; }

    public ICollection<Subtask> AuthoredSubtasks { get; set; }

    public ICollection<Subtask> AssignedSubtasks { get; set; }

    public ICollection<Comment> Comments { get; set; }
}