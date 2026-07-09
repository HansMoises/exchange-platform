using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.CrearIntercambio;

public record CrearIntercambioCommand(
    Guid ObjetoSolicitadoId,
    Guid ObjetoOfrecidoId,
    Guid UsuarioSolicitanteId,
    string? MensajeInicial) : IRequest<Guid>;