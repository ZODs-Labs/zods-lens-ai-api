
namespace ZODs.Api.Common.Configuration
{
    public sealed class GoogleAuthOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public string JavaScriptOrigin { get; set; }

        public string GeminiApiKey { get; set; }
    }
}
