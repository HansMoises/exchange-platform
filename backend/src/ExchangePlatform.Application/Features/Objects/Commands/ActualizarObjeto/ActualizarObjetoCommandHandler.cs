using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Commands.ActualizarObjeto;

public class ActualizarObjetoCommandHandler
    : IRequestHandler<ActualizarObjetoCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public ActualizarObjetoCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        ActualizarObjetoCommand request, CancellationToken ct)
    {
        var objeto = await _uow.Objetos.GetByIdAsync(request.ObjetoId)
            ?? throw new NotFoundException("Objeto no encontrado.");

        // RN-005: solo el propietario puede editar
        if (objeto.UsuarioId != request.UsuarioId)
            throw new ForbiddenException("No tienes permiso para editar este objeto.");

        objeto.Actualizar(
            request.Titulo,
            request.Descripcion,
            request.CategoriaId,
            request.CondicionFisica,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId);

        objeto.UpdatedBy = request.UsuarioId;
        _uow.Objetos.Update(objeto);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}