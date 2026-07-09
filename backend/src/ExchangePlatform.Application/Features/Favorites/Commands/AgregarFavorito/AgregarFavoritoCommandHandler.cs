using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Favorites.Commands.AgregarFavorito;

public class AgregarFavoritoCommandHandler
    : IRequestHandler<AgregarFavoritoCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public AgregarFavoritoCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(
        AgregarFavoritoCommand request, CancellationToken ct)
    {
        // RN-042: no duplicar favorito
        var existe = await _uow.Favoritos
            .GetByUsuarioYObjetoAsync(request.UsuarioId, request.ObjetoId);

        if (existe != null)
            throw new ConflictException("El objeto ya está en tus favoritos.");

        var objeto = await _uow.Objetos.GetByIdAsync(request.ObjetoId)
            ?? throw new NotFoundException("Objeto no encontrado.");

        var favorito = new Favorito(request.UsuarioId, request.ObjetoId);
        favorito.CreatedBy = request.UsuarioId;

        await _uow.Favoritos.AddAsync(favorito);
        await _uow.SaveChangesAsync(ct);

        return favorito.Id;
    }
}