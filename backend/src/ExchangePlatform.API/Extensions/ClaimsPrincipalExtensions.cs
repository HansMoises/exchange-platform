using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ExchangePlatform.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                 ?? user.FindFirst("sub")
                 ?? user.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claim == null || !Guid.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException("Usuario no autenticado.");

        return id;
    }
}