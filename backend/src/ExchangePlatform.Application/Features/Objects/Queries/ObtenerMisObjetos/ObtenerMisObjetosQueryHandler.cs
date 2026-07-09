using ExchangePlatform.Application.Features.Objects.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerMisObjetos;

public class ObtenerMisObjetosQueryHandler
    : IRequestHandler<ObtenerMisObjetosQuery, List<ObjetoDto>>
{
    private readonly IUnitOfWork _uow;

    public ObtenerMisObjetosQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<ObjetoDto>> Handle(
        ObtenerMisObjetosQuery request, CancellationToken ct)
    {
        var objetos = await _uow.Objetos.GetByUsuarioIdAsync(request.UsuarioId);
        var usuario = await _uow.Usuarios.GetByIdAsync(request.UsuarioId);
        var categorias = (await _uow.Categorias.GetActivasAsync())
            .ToDictionary(c => c.Id, c => c.Nombre);

        return objetos.Select(o =>
        {
            categorias.TryGetValue(o.CategoriaId, out var categoriaNombre);
            return new ObjetoDto
            {
                Id = o.Id,
                Titulo = o.Titulo,
                Descripcion = o.Descripcion,
                CategoriaId = o.CategoriaId,
                CategoriaNombre = categoriaNombre ?? "",
                UsuarioId = o.UsuarioId,
                UsuarioNombres = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}" : "",
                UsuarioCalificacion = usuario?.CalificacionPromedio ?? 0,
                Estado = o.Estado.ToString(),
                CondicionFisica = o.CondicionFisica,
                DepartamentoId = o.DepartamentoId,
                ProvinciaId = o.ProvinciaId,
                DistritoId = o.DistritoId,
                Imagenes = o.Imagenes.Select(i => new ImagenObjetoDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    Orden = i.Orden
                }).OrderBy(i => i.Orden).ToList(),
                CreadoEn = o.CreatedAt
            };
        }).ToList();
    }
}
