using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;
using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.App.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskEntity>> GetTasksAsync(int spaceId, int actorId, CancellationToken cancellationToken);

    Task<TaskEntity> CreateTaskAsync(
        int spaceId,
        int actorId,
        string title,
        string? description,
        TaskStatus? status,
        TaskPriority priority,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken);

    Task<TaskEntity> UpdateTaskAsync(
        int taskId,
        int actorId,
        string? title,
        string? description,
        TaskStatus? status,
        TaskPriority? priority,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken);

    Task<TaskEntity> UpdateTaskStatusAsync(int taskId, int actorId, TaskStatus status, CancellationToken cancellationToken);

    global::System.Threading.Tasks.Task DeleteTaskAsync(int taskId, int actorId, CancellationToken cancellationToken);
}