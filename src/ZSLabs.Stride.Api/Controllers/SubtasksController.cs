using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.App.Services;
using DomainComment = ZSLabs.Stride.Domain.Entities.Comment;
using DomainSubtask = ZSLabs.Stride.Domain.Entities.Subtask;

namespace ZSLabs.Stride.Api.Controllers;

[ApiController]
[Authorize(Policy = "RegularOnly")]
[Route("")]
public sealed class SubtasksController : ControllerBase
{
    private readonly ISubtaskService _subtaskService;

    public SubtasksController(ISubtaskService subtaskService)
    {
        _subtaskService = subtaskService;
    }

    [HttpPost("tasks/{taskId:int}/subtasks")]
    public async Task<ActionResult<Subtask>> CreateAsync(int taskId, [FromBody] CreateSubtaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var subtask = await _subtaskService.CreateSubtaskAsync(
                taskId,
                GetCurrentUserId(),
                request.Title,
                request.Description,
                request.Status is null ? null : (Domain.Enums.SubtaskStatus?)request.Status.Value,
                request.AssigneeId,
                request.DueDate,
                cancellationToken);

            return Created($"/subtasks/{subtask.Id}", Map(subtask));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ErrorResponse(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(exception.Message));
        }
    }

    [HttpPut("subtasks/{id:int}")]
    public async Task<ActionResult<Subtask>> UpdateAsync(int id, [FromBody] UpdateSubtaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var subtask = await _subtaskService.UpdateSubtaskAsync(
                id,
                GetCurrentUserId(),
                request.Title,
                request.Description,
                request.Status is null ? null : (Domain.Enums.SubtaskStatus?)request.Status.Value,
                request.AssigneeId,
                request.DueDate,
                cancellationToken);

            return Ok(Map(subtask));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ErrorResponse(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(exception.Message));
        }
    }

    [HttpDelete("subtasks/{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _subtaskService.DeleteSubtaskAsync(id, GetCurrentUserId(), cancellationToken);
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

    private static Subtask Map(DomainSubtask subtask)
    {
        return new Subtask(
            subtask.Id,
            subtask.TaskId,
            subtask.Title,
            subtask.Description,
            (Contracts.SubtaskStatus)subtask.Status,
            subtask.AuthorId,
            subtask.AssigneeId,
            subtask.DueDate,
            subtask.CreatedAt,
            subtask.UpdatedAt,
            subtask.Comments.OrderBy(comment => comment.CreatedAt).Select(Map).ToList());
    }

    private static Comment Map(DomainComment comment)
    {
        return new Comment(comment.Id, comment.TaskId, comment.SubtaskId, comment.AuthorId, comment.Content, comment.CreatedAt, comment.UpdatedAt);
    }
}