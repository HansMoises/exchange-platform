using ExchangePlatform.Application.Features.Notifications.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Notifications.Queries.ObtenerNotificaciones;

public class ObtenerNotificacionesQueryHandler
    : IRequestHandler<ObtenerNotificacionesQuery, List<NotificacionDto>>
{
    private readonly IUnitOfWork _uow;

    public ObtenerNotificacionesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<NotificacionDto>> Handle(
        ObtenerNotificacionesQuery request, CancellationToken ct)
    {
        var notificaciones = await _uow.Notificaciones
            .GetByUsuarioIdAsync(request.UsuarioId);

        return notificaciones.Select(n => new NotificacionDto
        {
            Id = n.Id,
            Tipo = n.Tipo.ToString(),
            Titulo = n.Titulo,
            Mensaje = n.Mensaje,
            IsLeida = n.IsLeida,
            EntidadTipo = n.EntidadTipo,
            EntidadId = n.EntidadId,
            CreadaEn = n.CreadaEn
        }).ToList();
    }
}