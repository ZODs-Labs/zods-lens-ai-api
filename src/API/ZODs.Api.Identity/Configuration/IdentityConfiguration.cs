namespace ZODs.Api.Identity.Configuration
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Key { get; set; }

        public string Provider { get; set; }

		public int ExpiryMinutes { get; set; }
    }

    public sealed class IdentityConfiguration
    {
        public JwtOptions JwtOptions { get; set; }
    }
}
