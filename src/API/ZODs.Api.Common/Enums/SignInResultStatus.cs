namespace ZODs.Api.Common.Enums
{
    public enum SignInResultStatus
    {
        Success,
        InvalidCredentials,
        EmailNotConfirmed,
        AccountLockedOut,
        TwoFactorRequired,
        Error
    }
}