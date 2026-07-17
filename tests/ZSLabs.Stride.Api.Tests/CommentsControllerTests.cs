using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using DomainComment = ZSLabs.Stride.Domain.Entities.Comment;

namespace ZSLabs.Stride.Api.Tests;

public class CommentsControllerTests
{
    [Fact]
    public void CommentsController_ClassAttribute_RequiresRegularOnlyPolicy()
    {
        var attribute = typeof(CommentsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("RegularOnly", attribute.Policy);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateTaskCommentAsync_ValidRequest_ReturnsCreatedComment()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ICommentService>();
        service.CreateTaskCommentAsync(4, 8, "Comment", cancellationToken)
            .Returns(new DomainComment(4, null, 8, "Comment", DateTime.UtcNow));

        var controller = CreateController(service, 8);
        var result = await controller.CreateTaskCommentAsync(4, new CreateCommentRequest("Comment"), cancellationToken);

        Assert.IsType<CreatedResult>(result.Result);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateAsync_ActorNotAuthor_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ICommentService>();
        service.UpdateCommentAsync(9, 8, "Updated", cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainComment>(new UnauthorizedAccessException("Only the author can modify this comment.")));

        var controller = CreateController(service, 8);
        var result = await controller.UpdateAsync(9, new CreateCommentRequest("Updated"), cancellationToken);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteAsync_ExistingComment_ReturnsNoContent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ICommentService>();
        var controller = CreateController(service, 8);

        var result = await controller.DeleteAsync(9, cancellationToken);

        Assert.IsType<NoContentResult>(result);
    }

    private static CommentsController CreateController(ICommentService service, int userId)
    {
        var controller = new CommentsController(service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                }, "Cookies")),
            },
        };

        return controller;
    }
}