using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.App.Services;
using UserContract = ZSLabs.Stride.Api.Contracts.User;

namespace ZSLabs.Stride.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType<List<UserContract>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserContract>>> GetAsync(CancellationToken cancellationToken)
    {
        var users = await _userService.GetRegularUsersAsync(cancellationToken);
        return Ok(users.Select(Map).ToList());
    }

    [HttpGet("/regular-users")]
    [Authorize(Policy = "RegularOnly")]
    [ProducesResponseType<List<RegularUserLookup>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegularUserLookup>>> GetRegularUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _userService.GetRegularUsersAsync(cancellationToken);
        return Ok(users.Select(user => new RegularUserLookup(user.Id, user.Username)).ToList());
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType<UserContract>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserContract>> CreateAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.CreateRegularUserAsync(request.Username, request.Password, request.Email, cancellationToken);
            return Created($"/users/{user.Id}", Map(user));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ErrorResponse(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType<UserContract>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserContract>> UpdateAsync(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateRegularUserAsync(id, request.Username, request.Password, request.Email, cancellationToken);
            return Ok(Map(user));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ErrorResponse(exception.Message));
        }
    }

    private static UserContract Map(ZSLabs.Stride.Domain.Entities.User user)
    {
        return new UserContract(user.Id, user.Username, user.Email, user.Role, user.CreatedAt);
    }
}