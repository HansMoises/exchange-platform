using AppValidationException = ExchangePlatform.Application.Common.Exceptions.ValidationException;
using ExchangePlatform.Application.Features.Auth.DTOs;
using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.RenovarToken;

public class RenovarTokenCommandHandler
    : IRequestHandler<RenovarTokenCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RenovarTokenCommandHandler(IUnitOfWork uow, IJwtService jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<LoginResponseDto> Handle(
        RenovarTokenCommand request, CancellationToken ct)
    {
        var refreshToken = await _uow.RefreshTokens
            .GetByTokenAsync(request.RefreshToken)
            ?? throw new AppValidationException(
                new List<string> { "Token de renovación inválido." });

        if (!refreshToken.EstaVigente())
            throw new AppValidationException(
                new List<string> { "Token de renovación expirado o revocado." });

        var usuario = await _uow.Usuarios.GetByIdAsync(refreshToken.UsuarioId)
            ?? throw new AppValidationException(
                new List<string> { "Usuario no encontrado." });

        // Rotación: revocar el token actual y emitir uno nuevo
        refreshToken.Revocar();
        _uow.RefreshTokens.Update(refreshToken);

        var nuevoRefreshTokenValue = _jwt.GenerarRefreshToken();
        var nuevoRefreshToken = new RefreshToken(
            nuevoRefreshTokenValue,
            usuario.Id,
            DateTime.UtcNow.AddDays(7),
            string.Empty);

        nuevoRefreshToken.CreatedBy = usuario.Id;
        await _uow.RefreshTokens.AddAsync(nuevoRefreshToken);

        var accessToken = _jwt.GenerarAccessToken(usuario);
        await _uow.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = nuevoRefreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Rol = RolesConocidos.ObtenerNombre(usuario.RolId),
                FotoPerfil = usuario.FotoPerfil,
                CalificacionPromedio = usuario.CalificacionPromedio,
                TotalIntercambios = usuario.TotalIntercambios
            }
        };
    }
}