using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Communities.Interfaces;
using SocialNetwork.Application.Communities.Requests;
using SocialNetwork.Application.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Communities;

public sealed class CommunityService(AppDbContext dbContext) : ICommunityService
{
    public async Task<IReadOnlyList<CommunityResponse>> GetPageAsync(
        Guid? userId,
        string? search,
        CancellationToken cancellationToken)
    {
        var normalizedSearch = search?.Trim();
        var query = dbContext.Communities
            .AsNoTracking()
            .Include(community => community.Members)
            .Where(community =>
                community.IsPublic ||
                (userId.HasValue &&
                 community.Members.Any(member => member.UserId == userId.Value)));

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
            query = query.Where(community =>
                community.Name.Contains(normalizedSearch) ||
                (community.Description != null &&
                 community.Description.Contains(normalizedSearch)));

        var communities = await query
            .OrderByDescending(community => community.CreatedAt)
            .ToListAsync(cancellationToken);

        return communities
            .Select(community => Map(community, userId))
            .ToList();
    }

    public async Task<CommunityResponse> GetByIdAsync(
        Guid id,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var community = await dbContext.Communities
            .AsNoTracking()
            .Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Community", id);

        if (!community.IsPublic &&
            (!userId.HasValue ||
             community.OwnerId != userId.Value &&
             !community.Members.Any(member => member.UserId == userId.Value)))
            throw new NotFoundException("Community", id);

        return Map(community, userId);
    }

    public async Task<CommunityResponse> CreateAsync(
        Guid ownerId,
        CreateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeName(request.Name);
        await EnsureNameAvailableAsync(name, null, cancellationToken);

        var community = new Community
        {
            OwnerId = ownerId,
            Name = name,
            Description = NormalizeOptional(request.Description),
            IsPublic = request.IsPublic
        };
        community.Members.Add(new CommunityMember { UserId = ownerId });
        dbContext.Communities.Add(community);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(community, ownerId);
    }

    public async Task<CommunityResponse> UpdateAsync(
        Guid id,
        Guid ownerId,
        UpdateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var community = await dbContext.Communities
            .Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Community", id);
        EnsureOwner(community, ownerId);

        var name = NormalizeName(request.Name);
        await EnsureNameAvailableAsync(name, id, cancellationToken);
        community.Name = name;
        community.Description = NormalizeOptional(request.Description);
        community.AvatarUrl = NormalizeOptional(request.AvatarUrl);
        community.CoverUrl = NormalizeOptional(request.CoverUrl);
        community.IsPublic = request.IsPublic;
        community.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(community, ownerId);
    }

    public async Task DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken)
    {
        var community = await dbContext.Communities
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Community", id);
        EnsureOwner(community, ownerId);
        dbContext.Communities.Remove(community);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task JoinAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var community = await dbContext.Communities
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Community", id);
        if (!community.IsPublic)
            throw new ForbiddenException("This community is private.");

        var alreadyMember = await dbContext.CommunityMembers.AnyAsync(
            member => member.CommunityId == id && member.UserId == userId,
            cancellationToken);
        if (!alreadyMember)
        {
            dbContext.CommunityMembers.Add(new CommunityMember
            {
                CommunityId = id,
                UserId = userId
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task LeaveAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var community = await dbContext.Communities
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Community", id);
        if (community.OwnerId == userId)
            throw new ConflictException("The community owner cannot leave the community.");

        var member = await dbContext.CommunityMembers
            .SingleOrDefaultAsync(
                item => item.CommunityId == id && item.UserId == userId,
                cancellationToken)
            ?? throw new ConflictException("You are not a member of this community.");
        dbContext.CommunityMembers.Remove(member);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameAvailableAsync(
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Communities.AnyAsync(
                community => community.Name == name &&
                    (!excludedId.HasValue || community.Id != excludedId.Value),
                cancellationToken))
            throw new ConflictException("A community with this name already exists.");
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 150)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Community name must be between 2 and 150 characters."]
            });
        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void EnsureOwner(Community community, Guid userId)
    {
        if (community.OwnerId != userId)
            throw new ForbiddenException("Only the community owner can modify this community.");
    }

    private static CommunityResponse Map(Community community, Guid? userId) =>
        new(
            community.Id,
            community.OwnerId,
            community.Name,
            community.Description,
            community.AvatarUrl,
            community.CoverUrl,
            community.IsPublic,
            community.Members.Count,
            userId.HasValue && community.Members.Any(member => member.UserId == userId.Value),
            community.CreatedAt,
            community.UpdatedAt);
}
