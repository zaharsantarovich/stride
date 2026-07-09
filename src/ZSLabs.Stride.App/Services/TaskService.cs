using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;
using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.App.Services;

public class TaskService : ITaskService
{
    private readonly StrideDbContext _dbContext;

    public TaskService(StrideDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaskEntity>> GetTasksAsync(int spaceId, int actorId, CancellationToken cancellationToken)
    {
        var space = await FindSpaceAsync(spaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        return await QueryTasks()
            .Where(task => task.SpaceId == spaceId)
            .OrderBy(task => task.Status)
            .ThenByDescending(task => task.Priority)
            .ThenBy(task => task.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity> CreateTaskAsync(
        int spaceId,
        int actorId,
        string title,
        string? description,
        TaskStatus? status,
        TaskPriority priority,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken)
    {
        var space = await FindSpaceAsync(spaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);
        await EnsureAssigneeHasSpaceAccessAsync(space, assigneeId, cancellationToken);

        var task = new TaskEntity(
            spaceId,
            title,
            description,
            status ?? TaskStatus.Backlog,
            priority,
            actorId,
            assigneeId,
            dueDate,
            DateTime.UtcNow);

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await LoadTaskGraphAsync(task.Id, cancellationToken);
    }

    public async Task<TaskEntity> UpdateTaskAsync(
        int taskId,
        int actorId,
        string? title,
        string? description,
        TaskStatus? status,
        TaskPriority? priority,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(taskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);
        await EnsureAssigneeHasSpaceAccessAsync(space, assigneeId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(title))
        {
            task.Title = title;
        }

        task.Description = description;
        task.AssigneeId = assigneeId;
        task.DueDate = dueDate;

        if (status.HasValue)
        {
            task.Status = status.Value;
        }

        if (priority.HasValue)
        {
            task.Priority = priority.Value;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await LoadTaskGraphAsync(task.Id, cancellationToken);
    }

    public async Task<TaskEntity> UpdateTaskStatusAsync(int taskId, int actorId, TaskStatus status, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(taskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await LoadTaskGraphAsync(task.Id, cancellationToken);
    }

    public async global::System.Threading.Tasks.Task DeleteTaskAsync(int taskId, int actorId, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(taskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TaskEntity> QueryTasks()
    {
        return _dbContext.Tasks
            .Include(task => task.Subtasks.OrderBy(subtask => subtask.CreatedAt))
            .ThenInclude(subtask => subtask.Comments.OrderBy(comment => comment.CreatedAt))
            .Include(task => task.Comments.OrderBy(comment => comment.CreatedAt));
    }

    private async Task<Space> FindSpaceAsync(int spaceId, CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .SingleOrDefaultAsync(space => space.Id == spaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Space not found.");
    }

    private async Task<TaskEntity> FindTaskAsync(int taskId, CancellationToken cancellationToken)
    {
        return await _dbContext.Tasks
            .SingleOrDefaultAsync(task => task.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");
    }

    private async Task<TaskEntity> LoadTaskGraphAsync(int taskId, CancellationToken cancellationToken)
    {
        return await QueryTasks()
            .SingleAsync(task => task.Id == taskId, cancellationToken);
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