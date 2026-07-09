using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Objects.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerObjetos;

public class ObtenerObjetosQueryHandler
    : IRequestHandler<ObtenerObjetosQuery, PagedResult<ObjetoDto>>
{
    private readonly IUnitOfWork _uow;

    public ObtenerObjetosQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<ObjetoDto>> Handle(
        ObtenerObjetosQuery request, CancellationToken ct)
    {
        var objetos = await _uow.Objetos.GetDisponiblesAsync(
            request.Search, request.CategoriaId,
            request.DepartamentoId, request.ProvinciaId,
            request.DistritoId, request.SortBy, request.SortOrder,
            request.PageNumber, request.PageSize);

        var total = await _uow.Objetos.CountDisponiblesAsync(
            request.Search, request.CategoriaId,
            request.DepartamentoId, request.ProvinciaId,
            request.DistritoId);

        var usuarioIds = objetos.Select(o => o.UsuarioId).Distinct().ToList();
        var usuarios = new Dictionary<Guid, Domain.Entities.Usuario>();
        foreach (var uid in usuarioIds)
        {
            var u = await _uow.Usuarios.GetByIdAsync(uid);
            if (u != null) usuarios[uid] = u;
        }

        var categorias = (await _uow.Categorias.GetActivasAsync())
            .ToDictionary(c => c.Id, c => c.Nombre);

        var items = objetos.Select(o =>
        {
            usuarios.TryGetValue(o.UsuarioId, out var usr);
            categorias.TryGetValue(o.CategoriaId, out var categoriaNombre);
            return new ObjetoDto
            {
                Id = o.Id,
                Titulo = o.Titulo,
                Descripcion = o.Descripcion,
                CategoriaId = o.CategoriaId,
                CategoriaNombre = categoriaNombre ?? "",
                UsuarioId = o.UsuarioId,
                UsuarioNombres = usr != null ? $"{usr.Nombres} {usr.Apellidos}" : "",
                UsuarioCalificacion = usr?.CalificacionPromedio ?? 0,
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

        return PagedResult<ObjetoDto>.Create(items, request.PageNumber,
            request.PageSize, total);
    }
}
