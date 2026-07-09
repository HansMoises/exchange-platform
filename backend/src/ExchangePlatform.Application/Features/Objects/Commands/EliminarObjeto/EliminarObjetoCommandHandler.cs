using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Commands.EliminarObjeto;

public class EliminarObjetoCommandHandler
    : IRequestHandler<EliminarObjetoCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public EliminarObjetoCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        EliminarObjetoCommand request, CancellationToken ct)
    {
        var objeto = await _uow.Objetos.GetByIdAsync(request.ObjetoId)
            ?? throw new NotFoundException("Objeto no encontrado.");

        // RN-005: solo el propietario puede eliminar
        if (objeto.UsuarioId != request.UsuarioId)
            throw new ForbiddenException("No tienes permiso para eliminar este objeto.");

        _uow.Objetos.Delete(objeto, request.UsuarioId);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}