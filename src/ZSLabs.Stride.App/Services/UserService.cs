using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;

namespace ZSLabs.Stride.App.Services;

public class UserService : IUserService
{
    private readonly StrideDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;

    public UserService(StrideDbContext dbContext, PasswordHashingService passwordHashingService)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<IReadOnlyList<User>> GetRegularUsersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Where(user => user.Role == UserRole.Regular)
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<User> CreateRegularUserAsync(string username, string password, string? email, CancellationToken cancellationToken)
    {
        await EnsureUniqueUsernameAsync(username, null, cancellationToken);

        var user = new User(username, string.Empty, email, UserRole.Regular, DateTime.UtcNow);
        user.PasswordHash = _passwordHashingService.HashPassword(user, password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<User> UpdateRegularUserAsync(int userId, string? username, string? password, string? email, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId && candidate.Role == UserRole.Regular, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(username) && !string.Equals(user.Username, username, StringComparison.Ordinal))
        {
            await EnsureUniqueUsernameAsync(username, userId, cancellationToken);
            user.Username = username;
        }

        if (password is not null)
        {
            user.PasswordHash = _passwordHashingService.HashPassword(user, password);
        }

        user.Email = email;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async global::System.Threading.Tasks.Task EnsureUniqueUsernameAsync(string username, int? userId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users.AnyAsync(
            user => user.Username == username && (!userId.HasValue || user.Id != userId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Username already exists.");
        }
    }
}