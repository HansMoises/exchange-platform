using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Application.Features.Objects.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerObjetoPorId;

public class ObtenerObjetoPorIdQueryHandler
    : IRequestHandler<ObtenerObjetoPorIdQuery, ObjetoDto>
{
    private readonly IUnitOfWork _uow;

    public ObtenerObjetoPorIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ObjetoDto> Handle(
        ObtenerObjetoPorIdQuery request, CancellationToken ct)
    {
        var o = await _uow.Objetos.GetByIdAsync(request.ObjetoId)
            ?? throw new NotFoundException("Objeto no encontrado.");

        var usuario = await _uow.Usuarios.GetByIdAsync(o.UsuarioId);
        var categoria = await _uow.Categorias.GetByIdAsync(o.CategoriaId);

        return new ObjetoDto
        {
            Id = o.Id,
            Titulo = o.Titulo,
            Descripcion = o.Descripcion,
            CategoriaId = o.CategoriaId,
            CategoriaNombre = categoria?.Nombre ?? "",
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
    }
}
