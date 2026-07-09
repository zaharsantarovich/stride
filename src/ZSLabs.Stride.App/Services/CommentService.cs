using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Persistence;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;

namespace ZSLabs.Stride.App.Services;

public class CommentService : ICommentService
{
    private readonly StrideDbContext _dbContext;

    public CommentService(StrideDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Comment>> GetTaskCommentsAsync(int taskId, int actorId, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(taskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        return await _dbContext.Comments
            .Where(comment => comment.TaskId == taskId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetSubtaskCommentsAsync(int subtaskId, int actorId, CancellationToken cancellationToken)
    {
        var subtask = await FindSubtaskAsync(subtaskId, cancellationToken);
        var task = await FindTaskAsync(subtask.TaskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        return await _dbContext.Comments
            .Where(comment => comment.SubtaskId == subtaskId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Comment> CreateTaskCommentAsync(int taskId, int actorId, string content, CancellationToken cancellationToken)
    {
        var task = await FindTaskAsync(taskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        var comment = new Comment(taskId, null, actorId, content, DateTime.UtcNow);
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment;
    }

    public async Task<Comment> CreateSubtaskCommentAsync(int subtaskId, int actorId, string content, CancellationToken cancellationToken)
    {
        var subtask = await FindSubtaskAsync(subtaskId, cancellationToken);
        var task = await FindTaskAsync(subtask.TaskId, cancellationToken);
        var space = await FindSpaceAsync(task.SpaceId, cancellationToken);
        EnsureCanAccessSpace(space, actorId);

        var comment = new Comment(null, subtaskId, actorId, content, DateTime.UtcNow);
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment;
    }

    public async Task<Comment> UpdateCommentAsync(int commentId, int actorId, string content, CancellationToken cancellationToken)
    {
        var comment = await FindCommentAsync(commentId, cancellationToken);
        EnsureIsAuthor(comment, actorId);

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment;
    }

    public async global::System.Threading.Tasks.Task DeleteCommentAsync(int commentId, int actorId, CancellationToken cancellationToken)
    {
        var comment = await FindCommentAsync(commentId, cancellationToken);
        EnsureIsAuthor(comment, actorId);

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskEntity> FindTaskAsync(int taskId, CancellationToken cancellationToken)
    {
        return await _dbContext.Tasks
            .SingleOrDefaultAsync(task => task.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");
    }

    private async Task<Subtask> FindSubtaskAsync(int subtaskId, CancellationToken cancellationToken)
    {
        return await _dbContext.Subtasks
            .SingleOrDefaultAsync(subtask => subtask.Id == subtaskId, cancellationToken)
            ?? throw new KeyNotFoundException("Subtask not found.");
    }

    private async Task<Space> FindSpaceAsync(int spaceId, CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .SingleOrDefaultAsync(space => space.Id == spaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Space not found.");
    }

    private async Task<Comment> FindCommentAsync(int commentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Comments
            .SingleOrDefaultAsync(comment => comment.Id == commentId, cancellationToken)
            ?? throw new KeyNotFoundException("Comment not found.");
    }

    private static void EnsureCanAccessSpace(Space space, int actorId)
    {
        if (!space.IsPublic && space.AuthorId != actorId)
        {
            throw new UnauthorizedAccessException("You do not have access to this space.");
        }
    }

    private static void EnsureIsAuthor(Comment comment, int actorId)
    {
        if (comment.AuthorId != actorId)
        {
            throw new UnauthorizedAccessException("Only the author can modify this comment.");
        }
    }
}