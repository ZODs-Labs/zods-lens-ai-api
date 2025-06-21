using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Identity;
using ZODs.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Dtos;

namespace ZODs.Api.Repository;

public sealed class UsersRepository : IUsersRepository
{
    private readonly ZodsContext dbContext;

    public UsersRepository(ZodsContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
            Expression<Func<User, bool>> predicate,
            Expression<Func<User, TResult>> selector,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = dbContext.Users.AsQueryable();

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.Where(predicate).Select(selector).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UserExistsAsync(
        Expression<Func<User, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Users.AnyAsync(predicate, cancellationToken).NoSync();
        return exists;
    }

    public async Task<(DateTime? expiresAt, bool isValid)> IsUserRefreshTokenValid(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return (null, false);
        }

        var token = await dbContext.RefreshTokens.Where(x => x.Token == refreshToken)
            .Select(x => new
            {
                x.ExpiresAt,
                IsValid = x.Revoked == null &&
                          x.ExpiresAt > DateTime.UtcNow
            })
            .FirstOrDefaultAsync(cancellationToken)
            .NoSync();

        return (token?.ExpiresAt, token?.IsValid ?? false);
    }

    public async Task<Guid> GetUserIdByRefreshToken(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var userId = await dbContext.Users.Where(x => x.RefreshTokens.Any(y => y.Token == refreshToken &&
                                                                               y.Revoked == null &&
                                                                               y.ExpiresAt > DateTime.UtcNow))
                                          .Select(x => x.Id)
                                          .FirstOrDefaultAsync(cancellationToken)
                                          .NoSync();

        return userId;
    }

    public async Task AddUserRefreshToken(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<string?> GetValidRefreshToken(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var validRefreshToken = await dbContext.RefreshTokens.Where(x => x.UserId == userId &&
                                                                         x.Revoked == null &&
                                                                         x.ExpiresAt > DateTime.UtcNow)
                                                             .Select(x => x.Token)
                                                             .FirstOrDefaultAsync(cancellationToken)
                                                             .NoSync();
        return validRefreshToken;
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? executingUserId,
        CancellationToken cancellationToken)
    {
        var userWithEmailExists = await dbContext.Users.AnyAsync(u => u.Id != executingUserId &&
                                                                      u.Email == email,
                                                                 cancellationToken).NoSync();

        return userWithEmailExists;
    }

    public async Task<bool> UserInvitedAsWorkspaceMemberExists(
        Guid workspaceMemberInviteId,
        CancellationToken cancellationToken)
    {
        var invitedUserEmail = await dbContext.WorkspaceMemberInvites.Where(x => x.Id == workspaceMemberInviteId)
                                                                     .Select(x => x.Email)
                                                                     .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(invitedUserEmail))
        {
            return false;
        }

        var userInvitedAsWorkspaceMemberExists = await dbContext.Users.AnyAsync(u => u.Email == invitedUserEmail,
                                                                                cancellationToken).NoSync();

        return userInvitedAsWorkspaceMemberExists;
    }

    public async Task<ICollection<UserFeatureDto>> GetUserFeaturesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userFeatures = await dbContext.UserFeatures.Where(x => x.UserId == userId)
                                                       .Select(x => new UserFeatureDto
                                                       {
                                                           FeatureIndex = x.Feature!.FeatureIndex,
                                                           Key = x.Feature!.Key,
                                                       })
                                                       .ToListAsync(cancellationToken)
                                                       .NoSync();

        return userFeatures;
    }

    public async Task AddUserFeatureAsync(
        Guid userId,
        FeatureIndex featureIndex,
        CancellationToken cancellationToken = default)
    {
        var feature = await dbContext.Features.FirstOrDefaultAsync(x => x.FeatureIndex == featureIndex, cancellationToken).NoSync();
        if (feature == null)
        {
            throw new KeyNotFoundException($"Feature with index {featureIndex} not found.");
        }

        var userFeature = new UserFeature
        {
            UserId = userId,
            FeatureId = feature.Id,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.UserFeatures.AddAsync(userFeature, cancellationToken);
    }
}
