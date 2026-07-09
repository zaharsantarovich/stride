using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Persistence;

namespace ZSLabs.Stride.App.Services;

public class AuthService : IAuthService
{
    private readonly StrideDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;

    public AuthService(StrideDbContext dbContext, PasswordHashingService passwordHashingService)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Username == username, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return _passwordHashingService.VerifyPassword(user, password, user.PasswordHash) ? user : null;
    }

    public Task<User?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }
}