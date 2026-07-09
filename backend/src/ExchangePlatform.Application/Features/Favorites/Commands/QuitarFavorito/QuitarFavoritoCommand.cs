using MediatR;

namespace ExchangePlatform.Application.Features.Favorites.Commands.QuitarFavorito;

public record QuitarFavoritoCommand(
    Guid UsuarioId,
    Guid ObjetoId) : IRequest<Unit>;