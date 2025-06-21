using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Common.Models;
using ZODs.Api.Extensions;
using ZODs.Api.Identity.Services.Interfaces;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service;
using ZODs.Api.Service.Dtos.User;
using ZODs.Api.Service.ResultDtos;
using ZODs.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Models.Result.Auth;
using ZODs.Api.Models.Input.Auth;

namespace ZODs.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
                    IUsersService usersService,
                    ITokenService tokenService,
                    ILogger<AuthController> logger,
                    UserManager<User> userManager,
                    SignInManager<User> signInManager,
                    IGoogleAuthService googleAuthService,
                    IWorkspaceService workspaceService,
                    IUserInfoService userInfoService,
                    IPricingPlanService pricingPlanService) : BaseController
    {
        private readonly IUsersService usersService = usersService;
        private readonly ITokenService tokenService = tokenService;
        private readonly ILogger<AuthController> logger = logger;
        private readonly UserManager<User> userManager = userManager;
        private readonly SignInManager<User> signInManager = signInManager;
        private readonly IGoogleAuthService googleAuthService = googleAuthService;
        private readonly IWorkspaceService workspaceService = workspaceService;
        private readonly IUserInfoService userInfoService = userInfoService;
        private readonly IPricingPlanService pricingPlanService = pricingPlanService;

        [AllowNoSubscription]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(
            [FromQuery] Guid? workspaceInviteId,
            [FromBody] SignUpDto dto,
            CancellationToken cancellationToken)
        {
            var userDto = new UserDto
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
            };

            UserCreateResultDto createResult;
            if (workspaceInviteId.HasValue)
            {
                // Workspace invited user flow
                createResult = await this.usersService.CreateWorkspaceInvitedUserAsync(
                    userDto,
                    dto.Password,
                    workspaceInviteId.Value,
                    cancellationToken).NoSync();

                // For workspace invited users, assign Basic Yearly plan
                // but don't mark them as subscribed, because we don't want
                // to charge workspace invited users
                await pricingPlanService.AssignNotPaidPricingPlanToUserAsync(
                    createResult.UserId,
                    PricingPlanVariantType.Yearly,
                    PricingPlanType.Basic,
                    cancellationToken).NoSync();

                await workspaceService.AcceptWorkspaceMemberInviteAsync(
                        workspaceInviteId.Value,
                        cancellationToken).NoSync();
            }
            else
            {
                userDto.RegistrationType = UserRegistrationType.RegularSignUp;

                // Regular user flow
                createResult = await this.usersService
                    .CreateUserAsync(
                        userDto,
                        dto.Password,
                        isEmailConfirmed: false,
                        cancellationToken)
                    .NoSync();

                // For regular sign-up flow - assign Free plan
                await pricingPlanService.AssignFreePricingPlanToUserAsync(createResult.UserId, cancellationToken);
            }

            if (!createResult.IsSuccess)
            {
                var errors = createResult.ErrorsFormatted;
                logger.LogError("Failed to create user. Reason: {reason}", errors);

                return BadRequest($"Failed to create user. Reason: {errors}");
            }

            var registrationResult = new RegistrationResult
            {
                UserId = createResult.UserId,
                IsSuccess = true,
            };

            return OkApiResponse(registrationResult);
        }

        [AllowNoSubscription]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] SignInDto dto, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(dto.EmailAddress);
            if (user == null)
            {
                return OkApiResponse(LoginResult.CreateInvalid());
            }

            var signInResult = await signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: false, lockoutOnFailure: false)
                .NoSync();
            if (!signInResult.Succeeded)
            {
                var loginResult = LoginResult.Create(signInResult, user.Id);
                return OkApiResponse(loginResult);
            }

            var customClaims = await GetUserInfoClaimsAsync(user.Id, cancellationToken);
            var jwtToken = await tokenService.GenerateJwtTokenAsync(user, customClaims).NoSync();

            await tokenService.SaveUserTokenAsync(user, jwtToken.Token);
            var refreshToken = await usersService.GenerateAndAddUserRefreshTokenAsync(user.Id, cancellationToken);
            var tokenResponse = new JwtTokenResponse
            {
                IdToken = jwtToken.Token,
                RefreshToken = refreshToken,
                ExpiresAt = jwtToken.ExpiresAt.ConvertUtcToUnixTimestamp(),
                UserId = user.Id.ToString(),
            };

            var result = LoginResult.Create(tokenResponse);

            return OkApiResponse(result);
        }

        [AllowNoSubscription]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return this.Ok();
        }

        // Delayed implemnetation of VS Code long-lived token
        [NonAction]
        [AllowNoSubscription]
        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken(
            [FromBody] GenerateTokenInputDto inputDto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var user = await userManager.FindByIdAsync(userId.ToString()).NoSync();
            if (user == null)
            {
                return Forbid();
            }

            var customClaims = await GetUserInfoClaimsAsync(userId, cancellationToken);
            var token = await tokenService.GenerateJwtTokenAsync(user, customClaims).NoSync();

            return OkApiResponse(token);
        }

        [AllowNoSubscription]
        [HttpGet("token/valid")]
        public IActionResult IsTokenValid()
        {
            var bearerToken = Request.GetBearerToken();
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                return BadRequest("Token not found.");
            }

            var securityToken = tokenService.ValidateCurrentToken(bearerToken);

            if (securityToken == null)
            {
                return BadRequest("Invalid token.");
            }

            var expiresAt = securityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value;

            return this.Ok(new
            {
                expiresAt = long.Parse(expiresAt),
                userId = User.GetUserId(),
            });
        }

        [AllowNoSubscription]
        [AllowAnonymous]
        [HttpGet("token/refresh")]
        public async Task<IActionResult> RefreshToken(
                [FromQuery] string refreshToken,
                CancellationToken cancellationToken)
        {
            var (expiresAt, isValid) = await usersService.IsUserRefreshTokenValidAsync(refreshToken, cancellationToken)
                                                         .NoSync();
            if (!isValid)
            {
                return BadRequest("Invalid refresh token.");
            }

            var userId = await usersService.GetUserIdByRefreshTokenAsync(refreshToken, cancellationToken).NoSync();
            var user = await userManager.FindByIdAsync(userId.ToString()).NoSync();

            var customClaims = await GetUserInfoClaimsAsync(user.Id, cancellationToken);
            var jwtToken = await tokenService.GenerateJwtTokenAsync(user, customClaims).NoSync();

            if (expiresAt != null && expiresAt.Value.AddMinutes(-30) <= DateTime.UtcNow)
            {
                // Generate new refresh token if the current one is about to expire
                refreshToken = await usersService.GenerateAndAddUserRefreshTokenAsync(user.Id, cancellationToken).NoSync();
            }

            return Ok(new JwtTokenResponse
            {
                UserId = user.Id.ToString(),
                IdToken = jwtToken.Token,
                RefreshToken = refreshToken,
                ExpiresAt = jwtToken.ExpiresAt.ConvertUtcToUnixTimestamp(),
            });
        }

        [AllowNoSubscription]
        [HttpGet("google")]
        public async Task<IActionResult> GoogleCallback(string code, CancellationToken cancellationToken)
        {
            var loginInfo = new UserLoginInfo("Google", code, "Google");
            var jwtTokenResponse = await ExternalGoogleLogin(loginInfo, cancellationToken).NoSync();

            if (jwtTokenResponse == null)
            {
                return BadRequest("Failed to login with Google.");
            }

            return Ok(jwtTokenResponse);
        }

        [AllowAnonymous]
        [HttpPost("email/verify")]
        public async Task<IActionResult> VerifyUserEmailAddress(
            [FromBody] VerifyUserEmailAddressInputDto inputDto)
        {
            var userId = inputDto.UserId;
            var code = inputDto.Code;

            var isVerified = await usersService.VerifyUserEmailAddressAsync(userId, code).NoSync();

            return OkApiResponse(new
            {
                isVerified,
            });
        }

        private async Task<JwtTokenResponse> ExternalGoogleLogin(
            UserLoginInfo info,
            CancellationToken cancellationToken)
        {
            if (info == null)
            {
                return null;
            }

            var signinResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false).NoSync();
            var googlePayload = await this.googleAuthService.AuthenticateWithAuthorizationCodeAsync(info.ProviderKey, cancellationToken).NoSync();
            if (googlePayload == null)
            {
                return null;
            }

            var email = googlePayload.Email;

            var user = await userManager.FindByEmailAsync(email).NoSync();

            if (signinResult.Succeeded)
            {
                var customClaims = await GetUserInfoClaimsAsync(user.Id, cancellationToken);
                var jwtToken = await tokenService.GenerateJwtTokenAsync(user, customClaims).NoSync();
                var refreshToken = await usersService.GenerateAndAddUserRefreshTokenAsync(user.Id, cancellationToken).NoSync();

                var loginResult = new JwtTokenResponse
                {
                    IdToken = jwtToken.Token,
                    RefreshToken = refreshToken,
                    ExpiresAt = jwtToken.ExpiresAt.ConvertUtcToUnixTimestamp(),
                    UserId = user.Id.ToString(),
                };

                return loginResult;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (user == null)
                {
                    var userDto = new UserDto
                    {
                        Email = email,
                        FirstName = googlePayload.GivenName,
                        LastName = googlePayload.FamilyName,
                        RegistrationType = UserRegistrationType.ExternalSignUp,
                    };

                    // We're confident that Google has already verified the email address
                    await usersService.CreateUserAsync(userDto, null, isEmailConfirmed: true, cancellationToken).NoSync();
                    user = await userManager.FindByEmailAsync(email).NoSync();

                    // Assign Free plan to new user
                    await pricingPlanService.AssignFreePricingPlanToUserAsync(user.Id, cancellationToken);
                }

                await userManager.AddLoginAsync(user, info).NoSync();
                await signInManager.SignInAsync(user, false).NoSync();

                var customClaims = await GetUserInfoClaimsAsync(user.Id, cancellationToken);
                var jwtResult = await tokenService.GenerateJwtTokenAsync(user, customClaims);
                var refreshToken = await usersService.GenerateAndAddUserRefreshTokenAsync(user.Id, cancellationToken);

                var loginResult = new JwtTokenResponse
                {
                    IdToken = jwtResult.Token,
                    RefreshToken = refreshToken,
                    ExpiresAt = jwtResult.ExpiresAt.ConvertUtcToUnixTimestamp(),
                    UserId = user.Id.ToString(),
                };

                return loginResult;
            }

            return null;
        }

        private async Task<Claim[]> GetUserInfoClaimsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userInfo = await userInfoService.GetUserInfoCachedAsync(userId, cancellationToken);
            var customClaims = new Claim[]
            {
                new(CustomClaims.PricingPlanType, userInfo.PricingPlanType?.ToString() ?? string.Empty),
                new(CustomClaims.IsSubscribed, userInfo.IsSubscribed.ToString().ToLower()),
                new(CustomClaims.HasValidSubscription, userInfo.HasValidSubscription.ToString().ToLower()),
                new(CustomClaims.IsPaidPlan, userInfo.IsPaidPlan.ToString().ToLower()),
            };

            return customClaims;
        }
    }
}
