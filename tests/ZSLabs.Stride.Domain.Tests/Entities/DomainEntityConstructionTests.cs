using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;
using UserRole = ZSLabs.Stride.Domain.Enums.UserRole;
using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.Domain.Tests.Entities;

public class DomainEntityConstructionTests
{
    [Fact]
    public void TaskConstructor_AllParameters_AssignsExpectedValues()
    {
        var createdAt = DateTime.UtcNow;
        var task = new ZSLabs.Stride.Domain.Entities.Task(
            12,
            "Ship feature",
            "Description",
            TaskStatus.Backlog,
            TaskPriority.High,
            3,
            4,
            createdAt.AddDays(1),
            createdAt);

        Assert.Equal(12, task.SpaceId);
        Assert.Equal("Ship feature", task.Title);
        Assert.Equal(TaskStatus.Backlog, task.Status);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal(3, task.AuthorId);
        Assert.Equal(4, task.AssigneeId);
    }

    [Fact]
    public void UserConstructor_AllParameters_AssignsExpectedValues()
    {
        var createdAt = DateTime.UtcNow;
        var user = new User("admin", "hash", "admin@example.com", UserRole.Admin, createdAt);

        Assert.Equal("admin", user.Username);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal(createdAt, user.CreatedAt);
    }
}