using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Persistence;

namespace ZSLabs.Stride.App.Services;

public class SpaceService : ISpaceService
{
    private readonly StrideDbContext _dbContext;

    public SpaceService(StrideDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Space>> GetVisibleSpacesAsync(int userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .Where(space => space.AuthorId == userId || space.IsPublic)
            .OrderBy(space => space.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Space> GetSpaceAsync(int spaceId, int userId, CancellationToken cancellationToken)
    {
        var space = await FindSpaceAsync(spaceId, cancellationToken);
        EnsureCanView(space, userId);
        return space;
    }

    public async Task<Space> CreateSpaceAsync(int authorId, string key, string name, bool isPublic, CancellationToken cancellationToken)
    {
        await EnsureUniqueKeyAsync(key, cancellationToken);

        var space = new Space(key, name, authorId, isPublic, DateTime.UtcNow);
        _dbContext.Spaces.Add(space);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return space;
    }

    public async Task<Space> UpdateSpaceAsync(int spaceId, int actorId, string? name, bool? isPublic, CancellationToken cancellationToken)
    {
        var space = await FindSpaceAsync(spaceId, cancellationToken);
        EnsureCanEdit(space, actorId, isPublic);

        if (!string.IsNullOrWhiteSpace(name))
        {
            space.Name = name;
        }

        if (isPublic.HasValue && space.AuthorId == actorId)
        {
            space.IsPublic = isPublic.Value;
        }

        space.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return space;
    }

    public async global::System.Threading.Tasks.Task DeleteSpaceAsync(int spaceId, int actorId, CancellationToken cancellationToken)
    {
        var space = await FindSpaceAsync(spaceId, cancellationToken);
        EnsureCanDelete(space, actorId);

        _dbContext.Spaces.Remove(space);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Space> FindSpaceAsync(int spaceId, CancellationToken cancellationToken)
    {
        return await _dbContext.Spaces
            .SingleOrDefaultAsync(space => space.Id == spaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Space not found.");
    }

    private async global::System.Threading.Tasks.Task EnsureUniqueKeyAsync(string key, CancellationToken cancellationToken)
    {
        if (await _dbContext.Spaces.AnyAsync(space => space.Key == key, cancellationToken))
        {
            throw new InvalidOperationException("Space key already exists.");
        }
    }

    private static void EnsureCanView(Space space, int actorId)
    {
        if (!space.IsPublic && space.AuthorId != actorId)
        {
            throw new UnauthorizedAccessException("You do not have access to this space.");
        }
    }

    private static void EnsureCanEdit(Space space, int actorId, bool? requestedVisibility)
    {
        EnsureCanView(space, actorId);

        if (requestedVisibility.HasValue && requestedVisibility.Value != space.IsPublic && space.AuthorId != actorId)
        {
            throw new UnauthorizedAccessException("Only the author can change space visibility.");
        }
    }

    private static void EnsureCanDelete(Space space, int actorId)
    {
        EnsureCanView(space, actorId);
    }
}