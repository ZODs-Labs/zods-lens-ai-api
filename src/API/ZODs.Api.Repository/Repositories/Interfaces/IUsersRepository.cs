using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Entities.Identity;
using System.Linq.Expressions;

namespace ZODs.Api.Repository;

public interface IUsersRepository
{
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<User, bool>> predicate,
        Expression<Func<User, TResult>> selector,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);

    Task AddUserRefreshToken(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    Task<(DateTime? expiresAt, bool isValid)> IsUserRefreshTokenValid(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<string?> GetValidRefreshToken(
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string email,
        Guid? executingUserId,
        CancellationToken cancellationToken);

    Task<bool> UserInvitedAsWorkspaceMemberExists(
        Guid workspaceMemberInviteId,
        CancellationToken cancellationToken);
    Task<Guid> GetUserIdByRefreshToken(string refreshToken, CancellationToken cancellationToken);
    Task AddUserFeatureAsync(Guid userId, FeatureIndex featureIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all user direct related features.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user features.</returns>
    Task<ICollection<UserFeatureDto>> GetUserFeaturesAsync(Guid userId, CancellationToken cancellationToken = default);
}