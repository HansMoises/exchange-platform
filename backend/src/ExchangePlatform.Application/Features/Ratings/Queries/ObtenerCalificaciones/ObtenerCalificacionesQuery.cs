using ExchangePlatform.Application.Features.Ratings.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Ratings.Queries.ObtenerCalificaciones;

public record ObtenerCalificacionesQuery(Guid UsuarioId) : IRequest<List<CalificacionDto>>;