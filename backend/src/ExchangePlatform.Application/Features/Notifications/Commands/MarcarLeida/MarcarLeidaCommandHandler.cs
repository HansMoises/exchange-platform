using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Notifications.Commands.MarcarLeida;

public class MarcarLeidaCommandHandler
    : IRequestHandler<MarcarLeidaCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public MarcarLeidaCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        MarcarLeidaCommand request, CancellationToken ct)
    {
        var notificacion = await _uow.Notificaciones
            .GetByIdAsync(request.NotificacionId)
            ?? throw new NotFoundException("Notificación no encontrada.");

        if (notificacion.UsuarioId != request.UsuarioId)
            throw new ForbiddenException("No tienes acceso a esta notificación.");

        notificacion.MarcarLeida();
        _uow.Notificaciones.Update(notificacion);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}