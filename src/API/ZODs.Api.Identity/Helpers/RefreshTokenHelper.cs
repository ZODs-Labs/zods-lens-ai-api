using System.Security.Cryptography;

namespace ZODs.Api.Identity.Helpers
{
    public static class RefreshTokenHelper
    {
        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// </summary>
        /// <param name="size">The size of the token to generate. Default is 32 bytes.</param>
        /// <returns>A URL-safe base64 encoded secure string.</returns>
        public static string GenerateSecureRefreshToken(int size = 32)
        {
            if (size < 16) // ensure at least 128 bits of entropy
            {
                throw new ArgumentException("Size must be at least 16 bytes for security reasons.");
            }

            var randomNumber = new byte[size];
            RandomNumberGenerator.Fill(randomNumber);

            string base64Token = Convert.ToBase64String(randomNumber);

            // Make the Base64 string URL-safe
            return base64Token.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}