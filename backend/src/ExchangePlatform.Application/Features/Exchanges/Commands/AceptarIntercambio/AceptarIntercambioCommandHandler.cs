using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.AceptarIntercambio;

public class AceptarIntercambioCommandHandler
    : IRequestHandler<AceptarIntercambioCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public AceptarIntercambioCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        AceptarIntercambioCommand request, CancellationToken ct)
    {
        var intercambio = await _uow.Intercambios
            .GetByIdAsync(request.IntercambioId)
            ?? throw new NotFoundException("Intercambio no encontrado.");

        // RN-023: solo el propietario acepta
        if (intercambio.UsuarioPropietarioId != request.UsuarioId)
            throw new ForbiddenException(
                "Solo el propietario puede aceptar la solicitud.");

        intercambio.Aceptar();
        intercambio.UpdatedBy = request.UsuarioId;

        // Reservar ambos objetos
        var objetoSolicitado = await _uow.Objetos
            .GetByIdAsync(intercambio.ObjetoSolicitadoId);
        var objetoOfrecido = await _uow.Objetos
            .GetByIdAsync(intercambio.ObjetoOfrecidoId);

        objetoSolicitado?.Reservar();
        objetoOfrecido?.Reservar();

        _uow.Intercambios.Update(intercambio);
        if (objetoSolicitado != null) _uow.Objetos.Update(objetoSolicitado);
        if (objetoOfrecido != null) _uow.Objetos.Update(objetoOfrecido);

        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}