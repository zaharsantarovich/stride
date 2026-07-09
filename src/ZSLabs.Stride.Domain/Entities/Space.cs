namespace ZSLabs.Stride.Domain.Entities;

public class Space
{
    public Space()
    {
        Tasks = new List<Task>();
    }

    public Space(string key, string name, int authorId, bool isPublic, DateTime createdAtUtc)
        : this()
    {
        Key = key;
        Name = name;
        AuthorId = authorId;
        IsPublic = isPublic;
        CreatedAt = createdAtUtc;
    }

    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int AuthorId { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public User? Author { get; set; }

    public ICollection<Task> Tasks { get; set; }
}