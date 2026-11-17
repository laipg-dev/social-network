using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class PermissionCatalogRepository : IPermissionCatalogRepository
{
    private readonly AppDbContext _dbContext;

    public PermissionCatalogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken = default)
    {
        if (codes.Count == 0)
        {
            return [];
        }

        return await _dbContext.Permissions
            .AsNoTracking()
            .Where(x => codes.Contains(x.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, Guid>> GetIdsByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken = default)
    {
        if (codes.Count == 0)
        {
            return new Dictionary<string, Guid>();
        }

        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .Where(x => codes.Contains(x.Code))
            .Select(x => new { x.Code, x.Id })
            .ToListAsync(cancellationToken);

        return permissions.ToDictionary(
            x => x.Code,
            x => x.Id);
    }
}
