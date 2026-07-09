using ExchangePlatform.Application.Features.Exchanges.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Queries.ObtenerIntercambios;

public record ObtenerIntercambiosQuery(
    Guid UsuarioId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<List<IntercambioDto>>;