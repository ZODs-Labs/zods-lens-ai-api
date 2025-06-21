using ZODs.Api.Identity.Models;
using ZODs.Api.Repository.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ZODs.Api.Identity.Services.Interfaces
{
    public interface ITokenService
    {
        Task<JwtToken> GenerateJwtTokenAsync(
            User user,
            ICollection<Claim> customClaims = null);
        Task<bool> SaveUserTokenAsync(User user, string token);
        JwtSecurityToken ValidateCurrentToken(string token);
    }
}