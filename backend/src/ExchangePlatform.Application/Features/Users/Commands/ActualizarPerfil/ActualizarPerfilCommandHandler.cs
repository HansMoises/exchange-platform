using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Users.Commands.ActualizarPerfil;

public class ActualizarPerfilCommandHandler
    : IRequestHandler<ActualizarPerfilCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public ActualizarPerfilCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        ActualizarPerfilCommand request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(request.UsuarioId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        usuario.ActualizarPerfil(
            request.Nombres,
            request.Apellidos,
            request.Telefono,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId);

        usuario.UpdatedBy = request.UsuarioId;
        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}