using System.Security.Claims;
using ChordHub.Api.Core.Models;

namespace ChordHub.Api.Shared.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);

    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
