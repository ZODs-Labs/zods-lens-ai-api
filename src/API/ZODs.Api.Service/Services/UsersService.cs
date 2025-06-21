using ZODs.Common.Exceptions;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Identity.Helpers;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Identity;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Service.Dtos.User;
using ZODs.Api.Service.Mappers;
using ZODs.Api.Service.ResultDtos;
using ZODs.Common.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZODs.Api.Service.InputDtos.User;
using ZODs.Api.Repository.Repositories.Interfaces;
using ZODs.Api.Repository.Dtos;
using ZODs.Payment.Clients;
using ZODs.Api.Common.Extensions;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Common.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace ZODs.Api.Service;

public sealed class UsersService : IUsersService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork;
    private readonly UserManager<User> userManager;
    private readonly IBackgroundTaskQueue backgroundTaskQueue;
    private readonly ILogger<UsersService> logger;
    private readonly IPaymentProcessorClient paymentProcessorClient;
    private readonly IEmailService emailService;
    private readonly ZodsApiConfiguration apiConfiguration;

    public UsersService(
        IUnitOfWork<ZodsContext> unitOfWork,
        UserManager<User> userManager,
        IBackgroundTaskQueue backgroundTaskQueue,
        ILogger<UsersService> logger,
        IPaymentProcessorClient paymentProcessorClient,
        IOptions<ZodsApiConfiguration> options,
        IEmailService emailService)
    {
        this.unitOfWork = unitOfWork;
        this.userManager = userManager;
        this.backgroundTaskQueue = backgroundTaskQueue;
        this.logger = logger;
        this.paymentProcessorClient = paymentProcessorClient;
        this.apiConfiguration = options.Value;
        this.emailService = emailService;
    }

    private IUsersRepository UsersRepository => this.unitOfWork.GetRepository<IUsersRepository>();
    private IPricingPlanRepository PricingPlanRepository => this.unitOfWork.GetRepository<IPricingPlanRepository>();
    private IWorkspaceMemberInvitesRepository WorkspaceMemberInvitesRepository => this.unitOfWork.GetRepository<IWorkspaceMemberInvitesRepository>();
    private ISnippetsRepository SnippetsRepository => this.unitOfWork.GetRepository<ISnippetsRepository>();
    private ISnippetTriggerPrefixesRepository SnippetTriggerPrefixesRepository => this.unitOfWork.GetRepository<ISnippetTriggerPrefixesRepository>();
    private IWorkspacesRepository WorkspacesRepository => this.unitOfWork.GetRepository<IWorkspacesRepository>();

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await this.userManager.FindByEmailAsync(email).NoSync();
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return user.ToDto();
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).NoSync();
        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {id} not found.");
        }

        return user.ToDto();
    }

    public async Task<UserCreateResultDto> CreateUserAsync(
        UserDto userDto,
        string password,
        bool isEmailConfirmed,
        CancellationToken cancellationToken)
    {
        await this.ValidateUserDto(userDto, cancellationToken).NoSync();

        logger.LogInformation("Creating user with email {email}.", userDto.Email);

        var user = userDto.ToEntity();
        user.EmailConfirmed = isEmailConfirmed;
        user.CreatedAt = DateTime.UtcNow;

        IdentityResult result;

        if (string.IsNullOrWhiteSpace(password))
        {
            result = await userManager.CreateAsync(user).NoSync();
        }
        else
        {
            result = await userManager.CreateAsync(user, password).NoSync();
        }

        if (result.Succeeded)
        {
            logger.LogInformation("User with email {email} created successfully.", userDto.Email);
            SendWelcomeMailToUser(user.Email!, user.FirstName, includeEmailVerification: !isEmailConfirmed);
        }
        else
        {
            logger.LogError("Error creating user with email {email}.", userDto.Email);
        }

        return UserCreateResultDto.FromIdentityResult(user.Id, result);
    }

    public async Task<UserCreateResultDto> CreateWorkspaceInvitedUserAsync(
        UserDto userDto,
        string password,
        Guid workspaceInviteId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating workspace invited user with email {email}.", userDto.Email);

        var inviteEmail = await WorkspaceMemberInvitesRepository.FirstOrDefaultAsync(
            x => x.Id == workspaceInviteId,
            selector: x => x.Email,
            cancellationToken: cancellationToken)
            .NoSync();

        if (string.IsNullOrWhiteSpace(inviteEmail))
        {
            throw new BusinessValidationException("Invalid workspace invite id.");
        }

        userDto.Email = inviteEmail;
        userDto.RegistrationType = UserRegistrationType.WorkspaceInvite;

        var userCreateResult = await CreateUserAsync(userDto, password, isEmailConfirmed: false, cancellationToken).NoSync();
        if (userCreateResult.IsSuccess)
        {
            await UsersRepository.AddUserFeatureAsync(userCreateResult.UserId, FeatureIndex.WorkspacesView, cancellationToken).NoSync();
        }

        return userCreateResult;
    }

    public async Task UpdateUserBillingDataAsync(
        Guid userId,
        UpdateUserBillingDataInputDto inputDto)
    {
        ArgumentNullException.ThrowIfNull(inputDto, nameof(inputDto));

        var user = await userManager.FindByIdAsync(userId.ToString()).NoSync();
        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {userId} not found.");
        }

        user.CardBrand = inputDto.CardBrand;
        user.CardLast4 = inputDto.CardLast4;
        user.UpdatePaymentMethodUrl = inputDto.UpdatePaymentMethodUrl;

        await userManager.UpdateAsync(user).NoSync();

        logger.LogInformation("User with id {userId} billing data updated successfully.", userId);
    }

    public async Task<IdentityResult> UpdateUserAsync(
        UserDto userDto,
        CancellationToken cancellationToken)
    {
        await this.ValidateUserDto(userDto, cancellationToken).NoSync();

        logger.LogInformation("Updating user with email {email}.", userDto.Email);

        var user = await userManager.FindByIdAsync(userDto.Id.ToString()!).NoSync();
        user!.Email = userDto.Email;
        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;

        var result = await userManager.UpdateAsync(user).NoSync();

        if (result.Succeeded)
        {
            logger.LogInformation("User with email {email} updated successfully.", userDto.Email);
        }
        else
        {
            var errors = result.FormatErrors();
            logger.LogError("Error updating user with email {email}. Reason: {reason}", userDto.Email, errors);
        }

        return result;
    }

    public async Task<string> GenerateAndAddUserRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var token = RefreshTokenHelper.GenerateSecureRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
        };

        await this.UsersRepository.AddUserRefreshToken(refreshToken, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        return token;
    }

    public async Task<string> RetrieveOrCreateValidRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var validRefreshToken = await this.UsersRepository.GetValidRefreshToken(userId, cancellationToken).NoSync();
        if (string.IsNullOrWhiteSpace(validRefreshToken))
        {
            validRefreshToken = await GenerateAndAddUserRefreshTokenAsync(userId, cancellationToken).NoSync();
        }

        return validRefreshToken;
    }

    public async Task<(DateTime? expiresAt, bool isValid)> IsUserRefreshTokenValidAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var token = await this.UsersRepository.IsUserRefreshTokenValid(refreshToken, cancellationToken).NoSync();

        return token;
    }

    public async Task<Guid> GetUserIdByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var userId = await this.UsersRepository.GetUserIdByRefreshToken(refreshToken, cancellationToken).NoSync();
        if (userId == Guid.Empty)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return userId;
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? executingUserId,
        CancellationToken cancellationToken)
    {
        var userWithEmailExists = await this.UsersRepository.EmailExistsAsync(email, executingUserId, cancellationToken).NoSync();
        return userWithEmailExists;
    }

    public async Task ResendConfirmationEmailAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()).NoSync();
        if (user == null)
        {
            throw new KeyNotFoundException(typeof(User).NotFoundValidationMessage(userId));
        }

        if (user.EmailConfirmed)
        {
            throw new BusinessValidationException("User email already confirmed.");
        }

        var emailConfirmationCode = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var emailVerificationUrl = ConstructEmailVerificationUrl(
            apiConfiguration.WebUrl,
            user.Id,
            emailConfirmationCode);

        await this.emailService.SendEmailConfirmationEmailAsync(
            user.Email,
            user.Email,
            user.FirstName,
            emailVerificationUrl);
    }

    public async Task<bool> VerifyUserEmailAddressAsync(
        Guid userId,
        string code)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()).NoSync();
        if (user == null)
        {
            throw new KeyNotFoundException(typeof(User).NotFoundValidationMessage(userId));
        }

        var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var identityResult = await userManager.ConfirmEmailAsync(user, decodedCode).NoSync();

        if (identityResult.Succeeded)
        {
            logger.LogInformation("User with id {userId} email address verified successfully.", userId);
        }
        else
        {
            logger.LogError("Error verifying user with id {userId} email address. Errors: {errors}", userId, JsonSerializer.Serialize(identityResult.Errors));
        }

        return identityResult.Succeeded;
    }

    public async Task<bool> UserInvitedAsWorkspaceMemberExists(
        Guid workspaceMemberInviteId,
        CancellationToken cancellationToken)
    {
        var userInvitedAsWorkspaceMemberExists = await this.UsersRepository.UserInvitedAsWorkspaceMemberExists(
            workspaceMemberInviteId,
            cancellationToken).NoSync();

        return userInvitedAsWorkspaceMemberExists;
    }

    public async Task<UserSubscriptionDto> GetUserSubscriptionInfoAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userSubscriptionInfo = await this.PricingPlanRepository.GetUserSubscriptionAsync(userId, cancellationToken).NoSync();
        if (userSubscriptionInfo == null)
        {
            throw new KeyNotFoundException($"Subscription for user with id {userId} not found.");
        }

        return userSubscriptionInfo;
    }

    public async Task DeleteUserAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()).NoSync();
        if (user == null)
        {
            throw new KeyNotFoundException(typeof(User).NotFoundValidationMessage(userId));
        }

        // 1) Cancel user subscription
        await CancelUserSubscriptionAsync(userId, cancellationToken).NoSync();

        // 2) Delete snippet trigger prefixes
        await this.SnippetTriggerPrefixesRepository.DeleteAllUserSnippetTriggerPrefixesAsync(userId, cancellationToken).NoSync();

        // 3) Delete user snippets
        await this.SnippetsRepository.DeleteAllUserSnippetsAsync(userId, cancellationToken).NoSync();

        // 4) Delete user workspaces and all related data
        await this.WorkspacesRepository.DeleteAllUserOwnedWorkspacesAsync(userId, cancellationToken).NoSync();

        // 5) Remove user membership from all workspaces
        await this.WorkspacesRepository.RemoveMemberFromAllWorkspacesAsync(userId, cancellationToken).NoSync();

        // 6) Delete user pricing plan 
        await this.PricingPlanRepository.DeleteUserPricingPlanAsync(userId, cancellationToken).NoSync();

        // 7) Delete user
        await userManager.DeleteAsync(user).NoSync();
    }

    private static string ConstructEmailVerificationUrl(string baseUrl, Guid userId, string emailConfirmationCode)
    {
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationCode));
        var url = $"{baseUrl}/auth/email/verify?userId={userId}&code={encodedCode}";

        return url;
    }

    private async Task ValidateUserDto(UserDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));

        var userWithEmailExists = await this.UsersRepository.EmailExistsAsync(dto.Email, dto.Id, cancellationToken).NoSync();
        if (userWithEmailExists)
        {
            throw new DuplicateEntityException($"User with email {dto.Email} already exists.");
        }
    }

    private void SendWelcomeMailToUser(string email, string firstName, bool includeEmailVerification = true)
    {
        backgroundTaskQueue.QueueBackgroundWorkItem(async (cancellationToken, serviceProvider) =>
        {
            using var scope = serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            if (includeEmailVerification)
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with email {email} not found.");
                }

                var emailConfirmationCode = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var emailVerificationUrl = ConstructEmailVerificationUrl(
                    apiConfiguration.WebUrl,
                    user.Id,
                    emailConfirmationCode);
                await emailService.SendWelcomeWithEmailVerificationMailAsync(
                    email,
                    email,
                    firstName,
                    emailVerificationUrl);
            }
            else
            {
                await emailService.SendWelcomeEmailAsync(
                    email,
                    email,
                    firstName);
            }
        });
    }

    private async Task CancelUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userSubscriptionId = await PricingPlanRepository.GetUserPaymentSubscriptionIdAsync(userId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(userSubscriptionId))
        {
            logger.LogInformation("Cancelling subscription for user with id {userId}.", userId);

            var payload = await paymentProcessorClient.CancelSubscriptionAsync(userSubscriptionId, cancellationToken);

            if (payload != null)
            {
                logger.LogInformation("Subscription for user with id {userId} cancelled successfully.", userId);
            }
        }
    }
}