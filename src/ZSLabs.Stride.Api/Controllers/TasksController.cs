using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.App.Services;
using DomainComment = ZSLabs.Stride.Domain.Entities.Comment;
using DomainSubtask = ZSLabs.Stride.Domain.Entities.Subtask;
using DomainTask = ZSLabs.Stride.Domain.Entities.Task;
using TaskContract = ZSLabs.Stride.Api.Contracts.Task;

namespace ZSLabs.Stride.Api.Controllers;

[ApiController]
[Authorize(Policy = "RegularOnly")]
[Route("")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("spaces/{spaceId:int}/tasks")]
    public async Task<ActionResult<IReadOnlyList<TaskContract>>> GetAsync(int spaceId, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _taskService.GetTasksAsync(spaceId, GetCurrentUserId(), cancellationToken);
            return Ok(tasks.Select(Map).ToList());
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

    [HttpPost("spaces/{spaceId:int}/tasks")]
    public async Task<ActionResult<TaskContract>> CreateAsync(int spaceId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskService.CreateTaskAsync(
                spaceId,
                GetCurrentUserId(),
                request.Title,
                request.Description,
                request.Status is null ? null : (Domain.Enums.TaskStatus?)request.Status.Value,
                (Domain.Enums.TaskPriority)request.Priority,
                request.AssigneeId,
                request.DueDate,
                cancellationToken);

            return Created($"/tasks/{task.Id}", Map(task));
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

    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<TaskContract>> UpdateAsync(int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskService.UpdateTaskAsync(
                id,
                GetCurrentUserId(),
                request.Title,
                request.Description,
                request.Status is null ? null : (Domain.Enums.TaskStatus?)request.Status.Value,
                request.Priority is null ? null : (Domain.Enums.TaskPriority?)request.Priority.Value,
                request.AssigneeId,
                request.DueDate,
                cancellationToken);

            return Ok(Map(task));
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

    [HttpPatch("tasks/{id:int}/status")]
    public async Task<ActionResult<TaskContract>> UpdateStatusAsync(int id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskService.UpdateTaskStatusAsync(id, GetCurrentUserId(), (Domain.Enums.TaskStatus)request.Status, cancellationToken);
            return Ok(Map(task));
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

    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _taskService.DeleteTaskAsync(id, GetCurrentUserId(), cancellationToken);
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

    private static TaskContract Map(DomainTask task)
    {
        return new TaskContract(
            task.Id,
            task.SpaceId,
            task.Title,
            task.Description,
            (Contracts.TaskStatus)task.Status,
            (Contracts.TaskPriority)task.Priority,
            task.AuthorId,
            task.AssigneeId,
            task.Assignee?.Username,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt,
            task.Subtasks.OrderBy(subtask => subtask.CreatedAt).Select(Map).ToList(),
            task.Comments.OrderBy(comment => comment.CreatedAt).Select(Map).ToList());
    }

    private static Contracts.Subtask Map(DomainSubtask subtask)
    {
        return new Contracts.Subtask(
            subtask.Id,
            subtask.TaskId,
            subtask.Title,
            subtask.Description,
            (Contracts.SubtaskStatus)subtask.Status,
            subtask.AuthorId,
            subtask.AssigneeId,
            subtask.Assignee?.Username,
            subtask.DueDate,
            subtask.CreatedAt,
            subtask.UpdatedAt,
            subtask.Comments.OrderBy(comment => comment.CreatedAt).Select(Map).ToList());
    }

    private static Contracts.Comment Map(DomainComment comment)
    {
        return new Contracts.Comment(comment.Id, comment.TaskId, comment.SubtaskId, comment.AuthorId, comment.Content, comment.CreatedAt, comment.UpdatedAt);
    }
}