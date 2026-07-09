using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.RestablecerPassword;

public class RestablecerPasswordCommandHandler
    : IRequestHandler<RestablecerPasswordCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public RestablecerPasswordCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<Unit> Handle(
        RestablecerPasswordCommand request, CancellationToken ct)
    {
        var resetToken = await _uow.PasswordResetTokens.GetByTokenAsync(request.Token);

        if (resetToken == null || !resetToken.EstaVigente())
            throw new ConflictException("El enlace ha expirado. Solicita uno nuevo.");

        var usuario = await _uow.Usuarios.GetByIdAsync(resetToken.UsuarioId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        usuario.ActualizarPassword(_hasher.Hash(request.Password));
        usuario.UpdatedBy = usuario.Id;
        _uow.Usuarios.Update(usuario);

        resetToken.MarcarUsado();
        _uow.PasswordResetTokens.Update(resetToken);

        // Medida de seguridad adicional: al restablecer la contrasena se
        // revocan todas las sesiones activas (refresh tokens) del usuario.
        await _uow.RefreshTokens.RevocarTodosDelUsuarioAsync(usuario.Id);

        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
