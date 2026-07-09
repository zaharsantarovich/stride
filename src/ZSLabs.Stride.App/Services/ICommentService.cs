using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.App.Services;

public interface ICommentService
{
    Task<IReadOnlyList<Comment>> GetTaskCommentsAsync(int taskId, int actorId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Comment>> GetSubtaskCommentsAsync(int subtaskId, int actorId, CancellationToken cancellationToken);

    Task<Comment> CreateTaskCommentAsync(int taskId, int actorId, string content, CancellationToken cancellationToken);

    Task<Comment> CreateSubtaskCommentAsync(int subtaskId, int actorId, string content, CancellationToken cancellationToken);

    Task<Comment> UpdateCommentAsync(int commentId, int actorId, string content, CancellationToken cancellationToken);

    global::System.Threading.Tasks.Task DeleteCommentAsync(int commentId, int actorId, CancellationToken cancellationToken);
}