using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;

namespace ZSLabs.Stride.App.Services;

public class SubtaskService : ISubtaskService
{
    private readonly StrideDbContext _dbContext;

    public SubtaskService(StrideDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Subtask> CreateSubtaskAsync(
        int taskId,
        int actorId,
        string title,
        string? description,
        SubtaskStatus? status,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(taskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);
        await EnsureAssigneeHasSpaceAccessAsync(space, assigneeId, cancellationToken);

        var subtask = new Subtask(
            taskId,
            title,
            description,
            status ?? SubtaskStatus.Todo,
            actorId,
            assigneeId,
            dueDate,
            DateTime.UtcNow);

        _dbContext.Subtasks.Add(subtask);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSubtaskGraphAsync(subtask.Id, cancellationToken);
    }

    public async Task<Subtask> UpdateSubtaskAsync(
        int subtaskId,
        int actorId,
        string? title,
        string? description,
        SubtaskStatus? status,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken)
    {
        var subtask = await FindSubtaskAsync(subtaskId, cancellationToken);
        var task = await FindTaskAsync(subtask.TaskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);
        await EnsureAssigneeHasSpaceAccessAsync(space, assigneeId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(title))
        {
            subtask.Title = title;
        }

        subtask.Description = description;
        subtask.AssigneeId = assigneeId;
        subtask.DueDate = dueDate;

        if (status.HasValue)
        {
            subtask.Status = status.Value;
        }

        subtask.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSubtaskGraphAsync(subtask.Id, cancellationToken);
    }

    public async global::System.Threading.Tasks.Task DeleteSubtaskAsync(int subtaskId, int actorId, CancellationToken cancellationToken)
    {
        var subtask = await FindSubtaskAsync(subtaskId, cancellationToken);
        var task = await FindTaskAsync(subtask.TaskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        _dbContext.Subtasks.Remove(subtask);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Subtask> QuerySubtasks()
    {
        return _dbContext.Subtasks
            .Include(subtask => subtask.Assignee)
            .Include(subtask => subtask.Comments.OrderBy(comment => comment.CreatedAt));
    }

    private async Task<TaskEntity> FindTaskAsync(int taskId, CancellationToken cancellationToken)
    {
        return await _dbContext.Tasks
            .SingleOrDefaultAsync(task => task.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");
    }

    private async Task<Space> FindSpaceAsync(int spaceId, CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .SingleOrDefaultAsync(space => space.Id == spaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Space not found.");
    }

    private async Task<Subtask> FindSubtaskAsync(int subtaskId, CancellationToken cancellationToken)
    {
        return await _dbContext.Subtasks
            .SingleOrDefaultAsync(subtask => subtask.Id == subtaskId, cancellationToken)
            ?? throw new KeyNotFoundException("Subtask not found.");
    }

    private async Task<Subtask> LoadSubtaskGraphAsync(int subtaskId, CancellationToken cancellationToken)
    {
        return await QuerySubtasks()
            .SingleAsync(subtask => subtask.Id == subtaskId, cancellationToken);
    }

    private async global::System.Threading.Tasks.Task EnsureAssigneeHasSpaceAccessAsync(Space space, int? assigneeId, CancellationToken cancellationToken)
    {
        if (!assigneeId.HasValue)
        {
            return;
        }

        var assignee = await _dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == assigneeId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Assignee does not have access to this space.");

        if (assignee.Role != UserRole.Regular)
        {
            throw new InvalidOperationException("Assignee does not have access to this space.");
        }

        if (!space.IsPublic && assignee.Id != space.AuthorId)
        {
            throw new InvalidOperationException("Assignee does not have access to this space.");
        }
    }

    private static void EnsureCanAccessSpace(Space space, int actorId)
    {
        if (!space.IsPublic && space.AuthorId != actorId)
        {
            throw new UnauthorizedAccessException("You do not have access to this space.");
        }
    }
}