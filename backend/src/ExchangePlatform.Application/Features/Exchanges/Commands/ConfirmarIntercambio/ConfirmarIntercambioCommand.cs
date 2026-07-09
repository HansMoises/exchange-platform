using ExchangePlatform.Application.Features.Exchanges.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.ConfirmarIntercambio;

public record ConfirmarIntercambioCommand(
    Guid IntercambioId,
    Guid UsuarioId) : IRequest<IntercambioDto>;