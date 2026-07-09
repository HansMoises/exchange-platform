using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Notifications.Commands.MarcarTodasLeidas;

public class MarcarTodasLeidasCommandHandler
    : IRequestHandler<MarcarTodasLeidasCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public MarcarTodasLeidasCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        MarcarTodasLeidasCommand request, CancellationToken ct)
    {
        await _uow.Notificaciones.MarcarTodasLeidasAsync(request.UsuarioId);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}