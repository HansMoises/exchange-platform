using ExchangePlatform.Application.Features.Objects.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Favorites.Queries.ObtenerFavoritos;

public class ObtenerFavoritosQueryHandler
    : IRequestHandler<ObtenerFavoritosQuery, List<ObjetoDto>>
{
    private readonly IUnitOfWork _uow;

    public ObtenerFavoritosQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<ObjetoDto>> Handle(
        ObtenerFavoritosQuery request, CancellationToken ct)
    {
        var favoritos = await _uow.Favoritos
            .GetByUsuarioIdAsync(request.UsuarioId);

        var categorias = (await _uow.Categorias.GetActivasAsync())
            .ToDictionary(c => c.Id, c => c.Nombre);
        var usuarios = new Dictionary<Guid, Domain.Entities.Usuario>();

        var items = new List<ObjetoDto>();

        foreach (var f in favoritos)
        {
            var objeto = await _uow.Objetos.GetByIdAsync(f.ObjetoId);
            if (objeto == null) continue;

            if (!usuarios.TryGetValue(objeto.UsuarioId, out var usr))
            {
                usr = await _uow.Usuarios.GetByIdAsync(objeto.UsuarioId);
                if (usr != null) usuarios[objeto.UsuarioId] = usr;
            }
            categorias.TryGetValue(objeto.CategoriaId, out var categoriaNombre);

            items.Add(new ObjetoDto
            {
                Id = objeto.Id,
                Titulo = objeto.Titulo,
                Descripcion = objeto.Descripcion,
                CategoriaId = objeto.CategoriaId,
                CategoriaNombre = categoriaNombre ?? "",
                UsuarioId = objeto.UsuarioId,
                UsuarioNombres = usr != null ? $"{usr.Nombres} {usr.Apellidos}" : "",
                UsuarioCalificacion = usr?.CalificacionPromedio ?? 0,
                Estado = objeto.Estado.ToString(),
                CondicionFisica = objeto.CondicionFisica,
                DepartamentoId = objeto.DepartamentoId,
                ProvinciaId = objeto.ProvinciaId,
                DistritoId = objeto.DistritoId,
                Imagenes = objeto.Imagenes.Select(i => new ImagenObjetoDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    Orden = i.Orden
                }).OrderBy(i => i.Orden).ToList(),
                CreadoEn = objeto.CreatedAt
            });
        }

        return items;
    }
}
