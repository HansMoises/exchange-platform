using MediatR;

namespace ExchangePlatform.Application.Features.Favorites.Commands.AgregarFavorito;

public record AgregarFavoritoCommand(
    Guid UsuarioId,
    Guid ObjetoId) : IRequest<Guid>;