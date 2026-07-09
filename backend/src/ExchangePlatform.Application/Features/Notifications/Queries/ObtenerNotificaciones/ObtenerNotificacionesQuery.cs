using ExchangePlatform.Application.Features.Notifications.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Notifications.Queries.ObtenerNotificaciones;

public record ObtenerNotificacionesQuery(Guid UsuarioId)
    : IRequest<List<NotificacionDto>>;