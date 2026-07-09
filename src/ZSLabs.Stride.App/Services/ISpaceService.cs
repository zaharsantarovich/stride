using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.App.Services;

public interface ISpaceService
{
    Task<IReadOnlyList<Space>> GetVisibleSpacesAsync(int userId, CancellationToken cancellationToken);

    Task<Space> GetSpaceAsync(int spaceId, int userId, CancellationToken cancellationToken);

    Task<Space> CreateSpaceAsync(int authorId, string key, string name, bool isPublic, CancellationToken cancellationToken);

    Task<Space> UpdateSpaceAsync(int spaceId, int actorId, string? name, bool? isPublic, CancellationToken cancellationToken);

    global::System.Threading.Tasks.Task DeleteSpaceAsync(int spaceId, int actorId, CancellationToken cancellationToken);
}