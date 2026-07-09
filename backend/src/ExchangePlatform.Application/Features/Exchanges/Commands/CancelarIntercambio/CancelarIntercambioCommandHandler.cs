using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.CancelarIntercambio;

public class CancelarIntercambioCommandHandler
    : IRequestHandler<CancelarIntercambioCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public CancelarIntercambioCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        CancelarIntercambioCommand request, CancellationToken ct)
    {
        var intercambio = await _uow.Intercambios
            .GetByIdAsync(request.IntercambioId)
            ?? throw new NotFoundException("Intercambio no encontrado.");

        if (intercambio.UsuarioSolicitanteId != request.UsuarioId &&
            intercambio.UsuarioPropietarioId != request.UsuarioId)
            throw new ForbiddenException("No eres parte de este intercambio.");

        intercambio.Cancelar();
        intercambio.UpdatedBy = request.UsuarioId;

        // Liberar objetos reservados
        var objetoSolicitado = await _uow.Objetos
            .GetByIdAsync(intercambio.ObjetoSolicitadoId);
        var objetoOfrecido = await _uow.Objetos
            .GetByIdAsync(intercambio.ObjetoOfrecidoId);

        objetoSolicitado?.LiberarReserva();
        objetoOfrecido?.LiberarReserva();

        _uow.Intercambios.Update(intercambio);
        if (objetoSolicitado != null) _uow.Objetos.Update(objetoSolicitado);
        if (objetoOfrecido != null) _uow.Objetos.Update(objetoOfrecido);

        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}