using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ExchangePlatform.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    // Default seguro: en producción la config viene de variables de entorno
    // (appsettings.json está gitignored y no viaja en la imagen de Render); si
    // falta la opcional Jwt__AccessTokenExpiryMinutes el login no debe caerse
    // con ArgumentNullException en int.Parse, sino usar este valor.
    private const int ExpiryMinutesPorDefecto = 15;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerarAccessToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, RolesConocidos.ObtenerNombre(usuario.RolId)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiraEnMinutos = int.TryParse(
            _config["Jwt:AccessTokenExpiryMinutes"], out var minutos)
                ? minutos
                : ExpiryMinutesPorDefecto;

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiraEnMinutos),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}