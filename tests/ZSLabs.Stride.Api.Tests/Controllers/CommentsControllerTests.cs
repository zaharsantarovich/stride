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
using DomainUser = ZSLabs.Stride.Domain.Entities.User;
using UserRole = ZSLabs.Stride.Domain.Enums.UserRole;

namespace ZSLabs.Stride.Api.Tests.Controllers;

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
        var author = new DomainUser("author", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 8 };
        service.CreateTaskCommentAsync(4, 8, "Comment", cancellationToken)
            .Returns(new DomainComment(4, null, 8, "Comment", DateTime.UtcNow) { Author = author });

        var controller = CreateController(service, 8);
        var result = await controller.CreateTaskCommentAsync(4, new CreateCommentRequest("Comment"), cancellationToken);

        var response = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal("author", Assert.IsType<Comment>(response.Value).AuthorUsername);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateAsync_OwnedComment_ReturnsMappedAuthorUsername()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ICommentService>();
        var author = new DomainUser("author", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 8 };
        service.UpdateCommentAsync(9, 8, "Updated", cancellationToken)
            .Returns(new DomainComment(4, null, 8, "Updated", DateTime.UtcNow) { Id = 9, Author = author, UpdatedAt = DateTime.UtcNow });

        var result = await CreateController(service, 8).UpdateAsync(9, new CreateCommentRequest("Updated"), cancellationToken);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("author", Assert.IsType<Comment>(response.Value).AuthorUsername);
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

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteAsync_ActorNotAuthor_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ICommentService>();
        service.DeleteCommentAsync(9, 8, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException(new UnauthorizedAccessException("Only the author can modify this comment.")));

        var result = await CreateController(service, 8).DeleteAsync(9, cancellationToken);

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
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