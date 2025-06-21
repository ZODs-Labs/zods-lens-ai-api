using ZODs.Api.Common.Constants;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ZODs.Api.Extensions
{
    public static class HttpRequestExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User id not found.");
            }

            return Guid.Parse(userId);
        }

        public static Guid? GetUserIdOptional(this ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userId, out var userIdOptional))
            {
                return userIdOptional;
            }

            return null;
        }

        public static string GetUserEmail(this ClaimsPrincipal claimsPrincipal)
        {
            var email = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            return email;
        }

        public static bool HasValidSubscriptionFlag(this ClaimsPrincipal claimsPrincipal)
        {
            var hasValidSubscriptionStringValue = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == CustomClaims.HasValidSubscription)?.Value;
            if (bool.TryParse(hasValidSubscriptionStringValue, out var hasValidSubscription))
            {
                return hasValidSubscription;
            }

            return false;
        }

        public static bool IsPaidPricingPlan(this ClaimsPrincipal claimsPrincipal)
        {
            var isPaidPricingPlanValue = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == CustomClaims.IsPaidPlan)?.Value;
            if (bool.TryParse(isPaidPricingPlanValue, out var isPaidPricingPLan))
            {
                return isPaidPricingPLan;
            }

            return false;
        }

        public static string GetBearerToken(this HttpRequest request)
        {
            var authorizationHeader = request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return string.Empty;
            }

            var tokenParts = authorizationHeader.Split(' ');
            if (tokenParts.Length != 2 || tokenParts[0] != "Bearer")
            {
                return string.Empty;
            }

            return tokenParts[1];
        }

        public static string GetPricingPlanType(this ClaimsPrincipal claimsPrincipal)
        {
            var pricingPlanType = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == CustomClaims.PricingPlanType)?.Value;

            return pricingPlanType;
        }

        public static string GetXSignatureFromHeader(this HttpRequest request)
        {
            if (!request.Headers.TryGetValue("X-Signature", out var signature))
            {
                throw new UnauthorizedAccessException("No signature header found.");
            }

            return signature;
        }

        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request)
        {
            request.EnableBuffering();
            string body;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            request.Body.Position = 0;
            return body;
        }

        public static void EnsureValidWebhookSignature(
            this HttpRequest request,
            string body,
            string signatureSecret)
        {
            var signatureHeader = request.GetXSignatureFromHeader();

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signatureSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

            if (!string.Equals(computedHashString, signatureHeader, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid signature detected: " + signatureHeader);
            }
        }
    }
}
