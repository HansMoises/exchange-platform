using MediatR;

namespace ExchangePlatform.Application.Features.Notifications.Commands.MarcarTodasLeidas;

public record MarcarTodasLeidasCommand(Guid UsuarioId) : IRequest<Unit>;