using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Favorites.Commands.QuitarFavorito;

public class QuitarFavoritoCommandHandler
    : IRequestHandler<QuitarFavoritoCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public QuitarFavoritoCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        QuitarFavoritoCommand request, CancellationToken ct)
    {
        var favorito = await _uow.Favoritos
            .GetByUsuarioYObjetoAsync(request.UsuarioId, request.ObjetoId)
            ?? throw new NotFoundException("El objeto no está en tus favoritos.");

        _uow.Favoritos.Delete(favorito, request.UsuarioId);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}