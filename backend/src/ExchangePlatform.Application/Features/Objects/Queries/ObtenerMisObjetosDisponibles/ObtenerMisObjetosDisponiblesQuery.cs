using ExchangePlatform.Application.Features.Objects.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerMisObjetosDisponibles;

public record ObtenerMisObjetosDisponiblesQuery(Guid UsuarioId)
    : IRequest<List<ObjetoDto>>;
