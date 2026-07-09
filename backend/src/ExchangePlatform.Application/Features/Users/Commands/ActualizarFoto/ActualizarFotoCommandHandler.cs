using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Users.Commands.ActualizarFoto;

public class ActualizarFotoCommandHandler : IRequestHandler<ActualizarFotoCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public ActualizarFotoCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(ActualizarFotoCommand request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(request.UsuarioId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        usuario.ActualizarFoto(request.Url);
        usuario.UpdatedBy = request.UsuarioId;
        _uow.Usuarios.Update(usuario);

        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
