using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.App.Services;
using SpaceContract = ZSLabs.Stride.Api.Contracts.Space;

namespace ZSLabs.Stride.Api.Controllers;

[ApiController]
[Authorize(Policy = "RegularOnly")]
[Route("spaces")]
public sealed class SpacesController : ControllerBase
{
    private readonly ISpaceService _spaceService;

    public SpacesController(ISpaceService spaceService)
    {
        _spaceService = spaceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SpaceContract>>> GetAsync(CancellationToken cancellationToken)
    {
        var spaces = await _spaceService.GetVisibleSpacesAsync(GetCurrentUserId(), cancellationToken);
        return Ok(spaces.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpaceContract>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var space = await _spaceService.GetSpaceAsync(id, GetCurrentUserId(), cancellationToken);
            return Ok(Map(space));
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

    [HttpPost]
    public async Task<ActionResult<SpaceContract>> CreateAsync([FromBody] CreateSpaceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var space = await _spaceService.CreateSpaceAsync(GetCurrentUserId(), request.Key, request.Name, request.IsPublic, cancellationToken);
            return Created($"/spaces/{space.Id}", Map(space));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ErrorResponse(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpaceContract>> UpdateAsync(int id, [FromBody] UpdateSpaceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var space = await _spaceService.UpdateSpaceAsync(id, GetCurrentUserId(), request.Name, request.IsPublic, cancellationToken);
            return Ok(Map(space));
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _spaceService.DeleteSpaceAsync(id, GetCurrentUserId(), cancellationToken);
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

    private static SpaceContract Map(ZSLabs.Stride.Domain.Entities.Space space)
    {
        return new SpaceContract(space.Id, space.Key, space.Name, space.AuthorId, space.IsPublic, space.CreatedAt);
    }
}