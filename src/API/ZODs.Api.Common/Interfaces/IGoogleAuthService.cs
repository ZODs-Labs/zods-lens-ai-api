using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace ZODs.Api.Common.Interfaces
{
    public interface IGoogleAuthService
    {
        /// <summary>
        /// Authenticates the user with the given authorization code.
        /// </summary>
        /// <param name="code">Authorization code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<Payload> AuthenticateWithAuthorizationCodeAsync(string code, CancellationToken cancellationToken);
    }
}
