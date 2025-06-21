using ZODs.Api.Identity.Configuration;
using ZODs.Api.Identity.Models;
using ZODs.Api.Identity.Services.Interfaces;
using ZODs.Api.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZODs.Api.Common.Constants;

namespace ZODs.Api.Identity.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtOptions _jwtConfig;
        private readonly TokenValidationParameters tokenValidationParams;

        public TokenService(UserManager<User> userManager, IOptions<IdentityConfiguration> options)
        {
            _userManager = userManager;
            _jwtConfig = options.Value.JwtOptions;

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Key);
            tokenValidationParams = new()
            {
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ClockSkew = TimeSpan.Zero,
            };

            if (string.IsNullOrWhiteSpace(_jwtConfig.Key))
            {
                throw new ArgumentNullException(nameof(_jwtConfig.Key), $"Key must be provided for JWT token");
            }
        }

        public async Task<JwtToken> GenerateJwtTokenAsync(
            User user,
            ICollection<Claim> customClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                new Claim(CustomClaims.IsEmailVerified, user.EmailConfirmed.ToString()),
            };

            // Fetch roles and add to claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var customClaim in customClaims ?? Array.Empty<Claim>())
            {
                claims.Add(customClaim);
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes);

            var token = new JwtSecurityToken(
                _jwtConfig.Issuer,
                _jwtConfig.Audience,
                claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new JwtToken
            {
                Token = tokenString,
                ExpiresAt = expiresAt,
            };
        }

        public JwtSecurityToken ValidateCurrentToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, tokenValidationParams, out SecurityToken validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;

            return jwtToken ?? throw new InvalidOperationException("The security token is not a JWT token.");
        }

        public static JwtToken ExtractTokenData(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.ReadToken(jwtToken) is not JwtSecurityToken token)
            {
                throw new ArgumentException("Invalid JWT token", nameof(jwtToken));
            }

            var userIdClaim = token.Claims.First(claim => claim.Type == "sub"); // or any custom UserId claim you might have used
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("UserId claim is missing in the JWT token.");
            }

            return new JwtToken
            {
                Token = jwtToken,
                UserId = userId,
                ExpiresAt = token.ValidTo,
            };
        }

        public async Task<bool> SaveUserTokenAsync(User user, string token)
        {
            var tokenModel = new IdentityUserToken<Guid>
            {
                UserId = user.Id,
                LoginProvider = _jwtConfig.Provider,
                Name = "JWT",
                Value = token,
            };

            await _userManager.SetAuthenticationTokenAsync(user, tokenModel.LoginProvider, tokenModel.Name, tokenModel.Value);
            return true;
        }
    }
}
