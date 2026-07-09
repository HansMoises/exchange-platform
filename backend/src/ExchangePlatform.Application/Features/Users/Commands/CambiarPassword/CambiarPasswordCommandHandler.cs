using AppValidationException = ExchangePlatform.Application.Common.Exceptions.ValidationException;
using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;

namespace ExchangePlatform.Application.Features.Users.Commands.CambiarPassword;

public class CambiarPasswordCommandHandler
    : IRequestHandler<CambiarPasswordCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public CambiarPasswordCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<Unit> Handle(
        CambiarPasswordCommand request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(request.UsuarioId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (!_hasher.Verificar(request.PasswordActual, usuario.PasswordHash))
            throw new AppValidationException(
                new List<string> { "La contraseña actual es incorrecta." });

        var nuevoHash = _hasher.Hash(request.PasswordNuevo);
        usuario.ActualizarPassword(nuevoHash);
        usuario.UpdatedBy = request.UsuarioId;

        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}