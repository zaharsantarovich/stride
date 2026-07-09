using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.App.Services;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetRegularUsersAsync(CancellationToken cancellationToken);

    Task<User> CreateRegularUserAsync(string username, string password, string? email, CancellationToken cancellationToken);

    Task<User> UpdateRegularUserAsync(int userId, string? username, string? password, string? email, CancellationToken cancellationToken);
}