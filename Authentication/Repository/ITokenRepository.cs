using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Authentication.Repository
{
    public interface ITokenRepository
    {
        string GenerateJWTToken(IdentityUser user, List<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
