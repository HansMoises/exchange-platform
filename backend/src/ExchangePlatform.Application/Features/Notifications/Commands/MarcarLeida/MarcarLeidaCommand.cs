using MediatR;

namespace ExchangePlatform.Application.Features.Notifications.Commands.MarcarLeida;

public record MarcarLeidaCommand(
    Guid NotificacionId,
    Guid UsuarioId) : IRequest<Unit>;