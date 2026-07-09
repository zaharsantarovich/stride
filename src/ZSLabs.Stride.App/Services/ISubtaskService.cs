using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.App.Services;

public interface ISubtaskService
{
    Task<Subtask> CreateSubtaskAsync(
        int taskId,
        int actorId,
        string title,
        string? description,
        SubtaskStatus? status,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken);

    Task<Subtask> UpdateSubtaskAsync(
        int subtaskId,
        int actorId,
        string? title,
        string? description,
        SubtaskStatus? status,
        int? assigneeId,
        DateTime? dueDate,
        CancellationToken cancellationToken);

    global::System.Threading.Tasks.Task DeleteSubtaskAsync(int subtaskId, int actorId, CancellationToken cancellationToken);
}