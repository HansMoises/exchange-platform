using ExchangePlatform.Application.Features.Objects.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Favorites.Queries.ObtenerFavoritos;

public record ObtenerFavoritosQuery(Guid UsuarioId) : IRequest<List<ObjetoDto>>;