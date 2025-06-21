using Microsoft.AspNetCore.Identity;
using ZODs.Api.Common.Enums;
using ZODs.Api.Common.Models;

namespace ZODs.Api.Models.Result.Auth;

public sealed class LoginResult
{
    public JwtTokenResponse Token { get; set; }

    public Guid? UserId { get; set; }

    public SignInResultStatus SignInResultStatus { get; set; }

    public bool IsSuccess => SignInResultStatus == SignInResultStatus.Success;

    public string ErrorMessage { get; set; }

    public static LoginResult Create(JwtTokenResponse token)
    {
        return new LoginResult
        {
            Token = token,
            SignInResultStatus = SignInResultStatus.Success
        };
    }

    public static LoginResult CreateInvalid()
    {
        return new LoginResult
        {
            SignInResultStatus = SignInResultStatus.InvalidCredentials,
            ErrorMessage = "Invalid email or password."
        };
    }

    public static LoginResult Create(SignInResult result, Guid? userId = null)
    {
        var loginResult = new LoginResult
        {
            UserId = userId,
        };

        if (result.Succeeded)
        {
            // Typically, you shouldn't reach this point in your method since
            // the intention is to cover non-successful scenarios.
            // If you do, you might consider returning null or throwing an exception.
            throw new ArgumentException("Expected a non-successful SignInResult.", nameof(result));
        }
        else if (result.IsLockedOut)
        {
            loginResult.SignInResultStatus = SignInResultStatus.AccountLockedOut;
            loginResult.ErrorMessage = "Your account is locked out due to multiple failed login attempts. Please try again in 5 mintues.";
        }
        else if (result.IsNotAllowed)
        {
            // This can often mean that the user hasn't confirmed their email or phone number
            // For a more precise check, you might need more context like checking the email confirmation status directly.
            loginResult.SignInResultStatus = SignInResultStatus.EmailNotConfirmed;
            loginResult.ErrorMessage = "Please confirm your email address before logging in.";
        }
        else if (result.RequiresTwoFactor)
        {
            loginResult.SignInResultStatus = SignInResultStatus.TwoFactorRequired;
            loginResult.ErrorMessage = "Please provide your two-factor authentication code.";
        }
        else
        {
            loginResult.SignInResultStatus = SignInResultStatus.InvalidCredentials;
            loginResult.ErrorMessage = "Invalid email or password.";
        }

        return loginResult;
    }
}
