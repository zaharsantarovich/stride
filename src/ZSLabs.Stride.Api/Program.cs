using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Middleware;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.Cookie.Name = "Stride.Auth";
		options.LoginPath = "/auth/login";
		options.AccessDeniedPath = "/auth/forbidden";
		options.SlidingExpiration = true;
		options.Events.OnRedirectToLogin = context =>
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return context.Response.WriteAsJsonAsync(new ErrorResponse("Authentication required."));
		};
		options.Events.OnRedirectToAccessDenied = context =>
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return context.Response.WriteAsJsonAsync(new ErrorResponse("Access denied."));
		};
	});
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("RegularOnly", policy => policy.RequireRole("Regular"));
});

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<PasswordHashingService>();
builder.Services.AddScoped<ISpaceService, SpaceService>();
builder.Services.AddScoped<ISubtaskService, SubtaskService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.Services.SeedAdminUserAsync(app.Configuration, app.Lifetime.ApplicationStopping);

app.Run();

public partial class Program
{
}
