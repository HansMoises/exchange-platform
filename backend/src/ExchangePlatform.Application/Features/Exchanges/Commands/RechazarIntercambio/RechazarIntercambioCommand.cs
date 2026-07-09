using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.RechazarIntercambio;

public record RechazarIntercambioCommand(
    Guid IntercambioId,
    Guid UsuarioId) : IRequest<Unit>;