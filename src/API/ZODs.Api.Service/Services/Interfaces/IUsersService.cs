using ZODs.Api.Service.Dtos.User;
using ZODs.Api.Service.ResultDtos;
using Microsoft.AspNetCore.Identity;
using ZODs.Api.Service.InputDtos.User;
using ZODs.Api.Repository.Dtos;

namespace ZODs.Api.Service;

public interface IUsersService
{
    /// <summary>
    /// Get user by id.
    /// </summary>
    /// <param name="id">User id.</param>
    /// <returns>User.</returns>
    Task<UserDto> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Creates a new User.
    /// </summary>
    /// <param name="userDto">User to create.</param>
    /// <param name="password">User password.</param>
    /// <param name="isEmailConfirmed">Flag indicating if email is confirmed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created User.</returns>
    Task<UserCreateResultDto> CreateUserAsync(UserDto userDto, string password, bool isEmailConfirmed, CancellationToken cancellationToken);

    Task<UserCreateResultDto> CreateWorkspaceInvitedUserAsync(
        UserDto userDto,
        string password,
        Guid workspaceInviteId,
        CancellationToken cancellationToken);

    Task<IdentityResult> UpdateUserAsync(
        UserDto userDto,
        CancellationToken cancellationToken);

    Task<UserDto> GetUserByEmailAsync(string email);

    Task<(DateTime? expiresAt, bool isValid)> IsUserRefreshTokenValidAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<string> GenerateAndAddUserRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<string> RetrieveOrCreateValidRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string email,
        Guid? executingUserId,
        CancellationToken cancellationToken);

    Task<bool> UserInvitedAsWorkspaceMemberExists(
        Guid workspaceMemberInviteId,
        CancellationToken cancellationToken);
    Task<Guid> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task UpdateUserBillingDataAsync(Guid userId, UpdateUserBillingDataInputDto inputDto);
    Task<UserSubscriptionDto> GetUserSubscriptionInfoAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteUserAccountAsync(Guid userId, CancellationToken cancellationToken);
    Task ResendConfirmationEmailAsync(Guid userId);
    Task<bool> VerifyUserEmailAddressAsync(Guid userId, string code);
}