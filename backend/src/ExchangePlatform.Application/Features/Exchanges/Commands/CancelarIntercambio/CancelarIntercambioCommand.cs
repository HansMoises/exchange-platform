using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.CancelarIntercambio;

public record CancelarIntercambioCommand(
    Guid IntercambioId,
    Guid UsuarioId) : IRequest<Unit>;