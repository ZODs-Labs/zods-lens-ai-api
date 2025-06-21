namespace ZODs.Api.Identity.Models
{
    public sealed class JwtToken
    {
        public string Token { get; set; }

        public string UserId { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}