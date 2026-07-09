using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.App.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);

    Task<User?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken);
}