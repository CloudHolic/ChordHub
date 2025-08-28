using System.Security.Claims;

namespace ChordHub.Api.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public static string GetEmail(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public static string GetDisplayName(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public static string? GetDiscordId(this ClaimsPrincipal principal) =>
        principal.FindFirst("discord_id")?.Value;
}
