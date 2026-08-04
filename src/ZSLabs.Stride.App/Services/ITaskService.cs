using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.App.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(int spaceId, int actorId, CancellationToken cancellationToken);

    Task<TaskItem> CreateTaskAsync(
        int spaceId,
        int actorId,
        string title,
        string? description,
        TaskStatus? status,
        TaskPriority priority,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken);

    Task<TaskItem> UpdateTaskAsync(
        int taskId,
        int actorId,
        string? title,
        string? description,
        TaskStatus? status,
        TaskPriority? priority,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken);

    Task<TaskItem> UpdateTaskStatusAsync(int taskId, int actorId, TaskStatus status, CancellationToken cancellationToken);

    Task DeleteTaskAsync(int taskId, int actorId, CancellationToken cancellationToken);
}
