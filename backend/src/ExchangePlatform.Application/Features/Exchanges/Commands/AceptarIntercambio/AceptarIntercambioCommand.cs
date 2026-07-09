using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.AceptarIntercambio;

public record AceptarIntercambioCommand(
    Guid IntercambioId,
    Guid UsuarioId) : IRequest<Unit>;