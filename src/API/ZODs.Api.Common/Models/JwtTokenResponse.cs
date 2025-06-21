namespace ZODs.Api.Common.Models
{
    public sealed class JwtTokenResponse
    {
        public string IdToken { get; set; }

        public string RefreshToken { get; set; }

        public string UserId { get; set; }

        public string EmailAddress { get; set; }

        public long ExpiresAt { get; set; }
    }
}
