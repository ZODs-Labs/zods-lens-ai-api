using ZODs.Api.Extensions;
using ZODs.Api.Service;
using ZODs.Api.Service.Dtos.User;
using ZODs.Common.Attributes;
using ZODs.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Models.Input.User;
using ZODs.Common.Models;

namespace ZODs.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUsersService usersService;
    private readonly IUserInfoService userInfoService;
    private readonly IPricingPlanService pricingPlanService;
    private readonly IUserAICreditBalanceService userAICreditBalanceService;
    private readonly IServiceProvider serviceProvider;

    public UsersController(
        IUsersService usersService,
        IUserInfoService userInfoService,
        IPricingPlanService pricingPlanService,
        IServiceProvider serviceProvider,
        IUserAICreditBalanceService userAICreditBalanceService)
    {
        this.usersService = usersService;
        this.userInfoService = userInfoService;
        this.pricingPlanService = pricingPlanService;
        this.serviceProvider = serviceProvider;
        this.userAICreditBalanceService = userAICreditBalanceService;
    }

    [AllowNoSubscription]
    [HttpGet("me")]
    public async Task<IActionResult> GetUserDetails()
    {
        var userId = User.GetUserId();
        var userDto = await usersService.GetUserByIdAsync(userId);

        return OkApiResponse(userDto);
    }

    [AllowNoSubscription]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateUserDetails(
        [FromBody] UserDto userDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        userDto.Id = userId;

        var result = await usersService.UpdateUserAsync(userDto, cancellationToken);
        if (!result.Succeeded)
        {
            var errors = result.FormatErrors();
            return BadRequest(ApiResponse.Error(errors, HttpStatusCode.UnprocessableEntity));
        }

        return OkApiResponse(userDto);
    }

    [AllowNoSubscription]
    [HttpGet("email/exists")]
    public async Task<IActionResult> EmailExists(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Ok(false);
        }

        var optionalUserId = User.GetUserIdOptional();
        var exists = await usersService.EmailExistsAsync(email, optionalUserId, cancellationToken);

        return Ok(exists);
    }

    [AllowAnonymous]
    [HttpPost("email/confirm/resend")]
    public async Task<IActionResult> ResendUserConfirmationEmail(
        [FromBody] ResendUserConfirmationEmailInputDto inputDto)
    {
        var userId = inputDto.UserId;
        await usersService.ResendConfirmationEmailAsync(userId);

        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("workspaces/invites/{workspaceInviteId}/user/exists")]
    public async Task<IActionResult> InvitedWorkspaceMemberExistsAsUser(
        [NotEmptyGuid] Guid workspaceInviteId,
        CancellationToken cancellationToken)
    {
        var invitedUserExists = await usersService.UserInvitedAsWorkspaceMemberExists(workspaceInviteId, cancellationToken);

        return OkApiResponse(new
        {
            InviteUserExists = invitedUserExists,
        });
    }

    [AllowNoSubscription]
    [HttpGet("me/info")]
    public async Task<IActionResult> GetUserInfo(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var userInfo = await userInfoService.GetUserInfoCachedAsync(userId, cancellationToken);

        return OkApiResponse(userInfo);
    }

    [AllowNoSubscription]
    [HttpGet("me/subscription")]
    public async Task<IActionResult> GetUserSubscriptionInfo(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var subscriptionInfo = await usersService.GetUserSubscriptionInfoAsync(userId, cancellationToken);

        return OkApiResponse(subscriptionInfo);
    }

    [HttpDelete("me/subscription")]
    public async Task<IActionResult> CancelUserSubscription(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await pricingPlanService.CancelUserSubscriptionAsync(userId, cancellationToken).NoSync();

        return Ok();
    }

    [AllowNoSubscription]
    [HttpDelete("me/account")]
    public async Task<IActionResult> DeleteUserAccount(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await usersService.DeleteUserAccountAsync(userId, cancellationToken);

        return Ok();
    }

    [AllowNoSubscription]
    [HttpGet("me/pricing-plan/usage")]
    public async Task<IActionResult> GetUserPricingPlanUsage(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var usage = await pricingPlanService.GetUserPricingPlanUsageAsync(userId, cancellationToken);

        return OkApiResponse(usage);
    }

    [AllowNoSubscription]
    [HttpGet("me/ai-credits/balance")]
    public async Task<IActionResult> GetUserAICreditsBalance(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var balance = await userAICreditBalanceService.GetUserAICreditsBalanceAsync(userId, cancellationToken).NoSync();

        return OkApiResponse(balance);
    }

    [AllowAnonymous]
    [AllowNoSubscription]
    [HttpGet("mail/test")]
    public async Task<IActionResult> SendTestEmail(
               CancellationToken cancellationToken)
    {
        var mailService = serviceProvider.GetRequiredService<IEmailService>();
        await mailService.SendWelcomeWithEmailVerificationMailAsync(
            "soleng@live.com",
            "soleng@live.com",
            "https://zods.pro/auth/email/verify",
            "soleng");

        await mailService.SendWorkspaceMemberInvitationEmailAsync(
            "soleng@live.com",
            "https://www.zods.com",
            "ZODs Toronto 1",
            cancellationToken);

        await mailService.SendWelcomeEmailAsync(
            "soleng@live.com",
            "soleng@live.com",
            "soleng");

        return Ok();
    }
}
