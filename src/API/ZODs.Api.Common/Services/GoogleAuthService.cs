using ZODs.Api.Common.Configuration;
using ZODs.Api.Common.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace ZODs.Api.Common.Services
{
    public sealed class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthOptions config;
        private readonly ClientSecrets secrets;
        private readonly ILogger<GoogleAuthService> logger;

        public GoogleAuthService(
            IOptions<GoogleAuthOptions> options,
            ILogger<GoogleAuthService> logger)
        {
            this.config = options.Value;
            if (config == null || string.IsNullOrWhiteSpace(config.ClientId))
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.secrets = this.GetClientSecrets();
            this.logger = logger;
        }

        public async Task<Payload> AuthenticateWithAuthorizationCodeAsync(string code, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new ArgumentNullException(nameof(code));
                }

                var flow = this.GetAuthorizationCodeFlow();

                // Exchange the Authorization Code for tokens
                var tokenResponse = await flow.ExchangeCodeForTokenAsync(
                   string.Empty,
                   code,
                   this.config.RedirectUri,
                   cancellationToken);

                // Use the tokens to get user info
                var payload = await ValidateAsync(tokenResponse.IdToken);
                return payload;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to authenticate with Google");
                return null;
            }
        }

        private GoogleAuthorizationCodeFlow GetAuthorizationCodeFlow()
        {
            return new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = this.secrets,
                    IncludeGrantedScopes = true,
                });
        }

        private ClientSecrets GetClientSecrets()
            => new()
            {
                ClientId = this.config.ClientId,
                ClientSecret = this.config.ClientSecret
            };
    }
}
