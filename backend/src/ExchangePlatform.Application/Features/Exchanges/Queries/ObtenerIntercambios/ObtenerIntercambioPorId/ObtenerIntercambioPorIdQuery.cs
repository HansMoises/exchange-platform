using ExchangePlatform.Application.Features.Exchanges.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Queries.ObtenerIntercambioPorId;

public record ObtenerIntercambioPorIdQuery(
    Guid IntercambioId,
    Guid UsuarioId) : IRequest<IntercambioDto>;