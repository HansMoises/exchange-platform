using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.RechazarIntercambio;

public class RechazarIntercambioCommandHandler
    : IRequestHandler<RechazarIntercambioCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public RechazarIntercambioCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        RechazarIntercambioCommand request, CancellationToken ct)
    {
        var intercambio = await _uow.Intercambios
            .GetByIdAsync(request.IntercambioId)
            ?? throw new NotFoundException("Intercambio no encontrado.");

        // RN-023: solo el propietario rechaza
        if (intercambio.UsuarioPropietarioId != request.UsuarioId)
            throw new ForbiddenException(
                "Solo el propietario puede rechazar la solicitud.");

        intercambio.Rechazar();
        intercambio.UpdatedBy = request.UsuarioId;

        _uow.Intercambios.Update(intercambio);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}