using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.App.Services;
using DomainComment = ZSLabs.Stride.Domain.Entities.Comment;

namespace ZSLabs.Stride.Api.Controllers;

[ApiController]
[Authorize(Policy = "RegularOnly")]
[Route("")]
public sealed class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost("tasks/{taskId:int}/comments")]
    public async Task<ActionResult<Comment>> CreateTaskCommentAsync(int taskId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var comment = await _commentService.CreateTaskCommentAsync(taskId, GetCurrentUserId(), request.Content, cancellationToken);
            return Created($"/comments/{comment.Id}", Map(comment));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(exception.Message));
        }
    }

    [HttpPost("subtasks/{subtaskId:int}/comments")]
    public async Task<ActionResult<Comment>> CreateSubtaskCommentAsync(int subtaskId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var comment = await _commentService.CreateSubtaskCommentAsync(subtaskId, GetCurrentUserId(), request.Content, cancellationToken);
            return Created($"/comments/{comment.Id}", Map(comment));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(exception.Message));
        }
    }

    [HttpPut("comments/{id:int}")]
    public async Task<ActionResult<Comment>> UpdateAsync(int id, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var comment = await _commentService.UpdateCommentAsync(id, GetCurrentUserId(), request.Content, cancellationToken);
            return Ok(Map(comment));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(exception.Message));
        }
    }

    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _commentService.DeleteCommentAsync(id, GetCurrentUserId(), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(exception.Message));
        }
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("Authentication required.");
    }

    private static Comment Map(DomainComment comment)
    {
        return new Comment(comment.Id, comment.TaskId, comment.SubtaskId, comment.AuthorId, comment.Content, comment.CreatedAt, comment.UpdatedAt);
    }
}