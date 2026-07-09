using AppValidationException = ExchangePlatform.Application.Common.Exceptions.ValidationException;
using ExchangePlatform.Application.Features.Auth.DTOs;
using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Application.Common.Exceptions;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.IniciarSesion;

public class IniciarSesionCommandHandler
    : IRequestHandler<IniciarSesionCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public IniciarSesionCommandHandler(
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtService jwt)
    {
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponseDto> Handle(
        IniciarSesionCommand request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByEmailAsync(request.Email)
            ?? throw new AppValidationException(
                new List<string> { "Credenciales inválidas." });

        if (usuario.EstaBloqueado())
            throw new ConflictException(
                "Cuenta bloqueada. Intenta nuevamente en 15 minutos.");

        if (!_hasher.Verificar(request.Password, usuario.PasswordHash))
        {
            usuario.RegistrarIntentoFallido();
            usuario.UpdatedBy = usuario.Id;
            _uow.Usuarios.Update(usuario);
            await _uow.SaveChangesAsync(ct);
            throw new AppValidationException(
                new List<string> { "Credenciales inválidas." });
        }

        usuario.ReiniciarIntentos();
        usuario.UpdatedBy = usuario.Id;
        _uow.Usuarios.Update(usuario);

        var accessToken = _jwt.GenerarAccessToken(usuario);
        var refreshTokenValue = _jwt.GenerarRefreshToken();

        var refreshToken = new RefreshToken(
            refreshTokenValue,
            usuario.Id,
            DateTime.UtcNow.AddDays(7),
            string.Empty);

        refreshToken.CreatedBy = usuario.Id;
        await _uow.RefreshTokens.AddAsync(refreshToken);
        await _uow.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
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