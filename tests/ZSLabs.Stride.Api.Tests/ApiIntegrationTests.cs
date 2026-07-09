using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ZSLabs.Stride.Api.Contracts;
using ApiTask = ZSLabs.Stride.Api.Contracts.Task;
using ApiTaskStatus = ZSLabs.Stride.Api.Contracts.TaskStatus;

namespace ZSLabs.Stride.Api.Tests;

public class ApiIntegrationTests : IAsyncLifetime
{
    private readonly StrideApiFactory _factory = new();

    [Fact]
    public async global::System.Threading.Tasks.Task SeededAdminCanManageUsersAndRegularUsersCannotAccessAdminEndpoints()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var username = $"alice-{suffix}";
        var renamedUsername = $"alice-renamed-{suffix}";
        using var adminClient = _factory.CreateApiClient();
        using var userClient = _factory.CreateApiClient();

        var admin = await LoginAsync(adminClient, "admin", "ChangeMe123!", cancellationToken);
        Assert.Equal("Admin", admin.Role.ToString());

        var createdUser = await PostAsync<CreateUserRequest, User>(adminClient, "/users", new CreateUserRequest(username, "Password123!", "alice@example.com"), HttpStatusCode.Created, cancellationToken);
        var users = await GetAsync<List<User>>(adminClient, "/users", cancellationToken);
        Assert.Contains(users, user => user.Id == createdUser.Id);

        var updatedUser = await PutAsync<UpdateUserRequest, User>(adminClient, $"/users/{createdUser.Id}", new UpdateUserRequest(renamedUsername, "Password456!", "alice2@example.com"), cancellationToken);
        Assert.Equal(renamedUsername, updatedUser.Username);

        await PostNoContentAsync(adminClient, "/auth/logout", cancellationToken);

        var loggedInRegular = await LoginAsync(userClient, renamedUsername, "Password456!", cancellationToken);
        Assert.Equal("Regular", loggedInRegular.Role.ToString());

        var denied = await userClient.GetAsync("/users", cancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task RegularUsersCanCollaborateOnSpacesWithVisibilityRules()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var aliceUsername = $"alice-{suffix}";
        var bobUsername = $"bob-{suffix}";
        using var adminClient = _factory.CreateApiClient();
        using var aliceClient = _factory.CreateApiClient();
        using var bobClient = _factory.CreateApiClient();

        await LoginAsync(adminClient, "admin", "ChangeMe123!", cancellationToken);
        var alice = await PostAsync<CreateUserRequest, User>(adminClient, "/users", new CreateUserRequest(aliceUsername, "Password123!", null), HttpStatusCode.Created, cancellationToken);
        var bob = await PostAsync<CreateUserRequest, User>(adminClient, "/users", new CreateUserRequest(bobUsername, "Password123!", null), HttpStatusCode.Created, cancellationToken);

        await LoginAsync(aliceClient, aliceUsername, "Password123!", cancellationToken);
        await LoginAsync(bobClient, bobUsername, "Password123!", cancellationToken);

        var privateSpace = await PostAsync<CreateSpaceRequest, Space>(aliceClient, "/spaces", new CreateSpaceRequest($"pri-{suffix}", "Private", false), HttpStatusCode.Created, cancellationToken);
        var publicSpace = await PostAsync<CreateSpaceRequest, Space>(aliceClient, "/spaces", new CreateSpaceRequest($"pub-{suffix}", "Public", true), HttpStatusCode.Created, cancellationToken);

        var bobSpaces = await GetAsync<List<Space>>(bobClient, "/spaces", cancellationToken);
        Assert.DoesNotContain(bobSpaces, space => space.Id == privateSpace.Id);
        Assert.Contains(bobSpaces, space => space.Id == publicSpace.Id);

        var privateDenied = await bobClient.GetAsync($"/spaces/{privateSpace.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, privateDenied.StatusCode);

        var renamed = await PutAsync<UpdateSpaceRequest, Space>(bobClient, $"/spaces/{publicSpace.Id}", new UpdateSpaceRequest("Renamed Public", true), cancellationToken);
        Assert.Equal("Renamed Public", renamed.Name);

        var visibilityDenied = await bobClient.PutAsJsonAsync($"/spaces/{publicSpace.Id}", new UpdateSpaceRequest("Still Public", false), cancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, visibilityDenied.StatusCode);

        var deleted = await bobClient.DeleteAsync($"/spaces/{publicSpace.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task RegularUsersCanManageBoardTasksSubtasksAndComments()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var aliceUsername = $"alice-{suffix}";
        var bobUsername = $"bob-{suffix}";
        using var adminClient = _factory.CreateApiClient();
        using var aliceClient = _factory.CreateApiClient();
        using var bobClient = _factory.CreateApiClient();

        await LoginAsync(adminClient, "admin", "ChangeMe123!", cancellationToken);
        var alice = await PostAsync<CreateUserRequest, User>(adminClient, "/users", new CreateUserRequest(aliceUsername, "Password123!", null), HttpStatusCode.Created, cancellationToken);
        var bob = await PostAsync<CreateUserRequest, User>(adminClient, "/users", new CreateUserRequest(bobUsername, "Password123!", null), HttpStatusCode.Created, cancellationToken);

        await LoginAsync(aliceClient, aliceUsername, "Password123!", cancellationToken);
        await LoginAsync(bobClient, bobUsername, "Password123!", cancellationToken);

        var space = await PostAsync<CreateSpaceRequest, Space>(aliceClient, "/spaces", new CreateSpaceRequest($"board-{suffix}", "Board", true), HttpStatusCode.Created, cancellationToken);

        var lowerTask = await PostAsync<CreateTaskRequest, ApiTask>(aliceClient, $"/spaces/{space.Id}/tasks",
            new CreateTaskRequest("Low", "desc", null, TaskPriority.Low, null, null), HttpStatusCode.Created, cancellationToken);
        var higherTask = await PostAsync<CreateTaskRequest, ApiTask>(aliceClient, $"/spaces/{space.Id}/tasks",
            new CreateTaskRequest("High", "desc", null, TaskPriority.High, bob.Id, null), HttpStatusCode.Created, cancellationToken);

        var listedTasks = await GetAsync<List<ApiTask>>(aliceClient, $"/spaces/{space.Id}/tasks", cancellationToken);
        Assert.Equal("High", listedTasks[0].Title);
        Assert.Equal("Low", listedTasks[1].Title);
        Assert.Equal(ApiTaskStatus.Backlog, listedTasks[0].Status);

        var patchedTask = await PatchAsync<UpdateTaskStatusRequest, ApiTask>(aliceClient, $"/tasks/{higherTask.Id}/status", new UpdateTaskStatusRequest(ApiTaskStatus.Done), cancellationToken);
        Assert.Equal(ApiTaskStatus.Done, patchedTask.Status);

        var updatedTask = await PutAsync<UpdateTaskRequest, ApiTask>(aliceClient, $"/tasks/{higherTask.Id}",
            new UpdateTaskRequest("High Updated", "new desc", ApiTaskStatus.InProgress, TaskPriority.Critical, bob.Id, DateTime.UtcNow.AddDays(1)), cancellationToken);
        Assert.Equal("High Updated", updatedTask.Title);
        Assert.Equal(TaskPriority.Critical, updatedTask.Priority);

        var subtask = await PostAsync<CreateSubtaskRequest, Subtask>(aliceClient, $"/tasks/{higherTask.Id}/subtasks",
            new CreateSubtaskRequest("Subtask", "sub desc", null, bob.Id, null), HttpStatusCode.Created, cancellationToken);
        Assert.Equal(SubtaskStatus.Todo, subtask.Status);

        var updatedSubtask = await PutAsync<UpdateSubtaskRequest, Subtask>(aliceClient, $"/subtasks/{subtask.Id}",
            new UpdateSubtaskRequest("Subtask 2", "changed", SubtaskStatus.Done, bob.Id, null), cancellationToken);
        Assert.Equal(SubtaskStatus.Done, updatedSubtask.Status);

        var taskComment = await PostAsync<CreateCommentRequest, Comment>(aliceClient, $"/tasks/{higherTask.Id}/comments", new CreateCommentRequest("Task comment"), HttpStatusCode.Created, cancellationToken);
        var subtaskComment = await PostAsync<CreateCommentRequest, Comment>(aliceClient, $"/subtasks/{subtask.Id}/comments", new CreateCommentRequest("Subtask comment"), HttpStatusCode.Created, cancellationToken);

        var updatedComment = await PutAsync<CreateCommentRequest, Comment>(aliceClient, $"/comments/{taskComment.Id}", new CreateCommentRequest("Task comment updated"), cancellationToken);
        Assert.Equal("Task comment updated", updatedComment.Content);

        var forbiddenCommentEdit = await bobClient.PutAsJsonAsync($"/comments/{taskComment.Id}", new CreateCommentRequest("hack"), cancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenCommentEdit.StatusCode);

        var boardTasks = await GetAsync<List<ApiTask>>(aliceClient, $"/spaces/{space.Id}/tasks", cancellationToken);
        var hydratedTask = Assert.Single(boardTasks, task => task.Id == higherTask.Id);
        Assert.Contains(hydratedTask.Comments, comment => comment.Id == taskComment.Id);
        Assert.Contains(hydratedTask.Subtasks, item => item.Id == subtask.Id);
        Assert.Contains(hydratedTask.Subtasks.SelectMany(item => item.Comments), comment => comment.Id == subtaskComment.Id);

        var deletedComment = await aliceClient.DeleteAsync($"/comments/{taskComment.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deletedComment.StatusCode);
        var deletedSubtask = await aliceClient.DeleteAsync($"/subtasks/{subtask.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deletedSubtask.StatusCode);
        var deletedTask = await aliceClient.DeleteAsync($"/tasks/{higherTask.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deletedTask.StatusCode);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();

        if (File.Exists(_factory.DatabasePath))
        {
            File.Delete(_factory.DatabasePath);
        }
    }

    private static async global::System.Threading.Tasks.Task<CurrentUser> LoginAsync(HttpClient client, string username, string password, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest(username, password), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadAsync<CurrentUser>(response, cancellationToken);
    }

    private static async global::System.Threading.Tasks.Task<TResponse> GetAsync<TResponse>(HttpClient client, string path, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async global::System.Threading.Tasks.Task<TResponse> PostAsync<TRequest, TResponse>(HttpClient client, string path, TRequest request, HttpStatusCode expectedStatus, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync(path, request, cancellationToken);
        Assert.Equal(expectedStatus, response.StatusCode);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async global::System.Threading.Tasks.Task<TResponse> PutAsync<TRequest, TResponse>(HttpClient client, string path, TRequest request, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync(path, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async global::System.Threading.Tasks.Task<TResponse> PatchAsync<TRequest, TResponse>(HttpClient client, string path, TRequest request, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, path)
        {
            Content = JsonContent.Create(request),
        };

        var response = await client.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async global::System.Threading.Tasks.Task PostNoContentAsync(HttpClient client, string path, CancellationToken cancellationToken)
    {
        var response = await client.PostAsync(path, null, cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async global::System.Threading.Tasks.Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        Assert.NotNull(value);
        return value;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private sealed class StrideApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"stride-tests-{Guid.NewGuid():N}.db");

        public HttpClient CreateApiClient()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Stride"] = $"Data Source={_databasePath}",
                    ["Admin:Username"] = "admin",
                    ["Admin:Password"] = "ChangeMe123!",
                    ["Admin:Email"] = "admin@test.local",
                });
            });
        }

        public string DatabasePath => _databasePath;
    }
}