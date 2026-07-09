using ExchangePlatform.Application.Features.Ratings.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Ratings.Queries.ObtenerCalificaciones;

public class ObtenerCalificacionesQueryHandler
    : IRequestHandler<ObtenerCalificacionesQuery, List<CalificacionDto>>
{
    private readonly IUnitOfWork _uow;

    public ObtenerCalificacionesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<CalificacionDto>> Handle(
        ObtenerCalificacionesQuery request, CancellationToken ct)
    {
        var calificaciones = await _uow.Calificaciones
            .GetByCalificadoIdAsync(request.UsuarioId);

        var items = new List<CalificacionDto>();

        foreach (var c in calificaciones)
        {
            var calificador = await _uow.Usuarios.GetByIdAsync(c.CalificadorId);
            items.Add(new CalificacionDto
            {
                Id = c.Id,
                IntercambioId = c.IntercambioId,
                CalificadorId = c.CalificadorId,
                CalificadorNombres = calificador != null
                    ? $"{calificador.Nombres} {calificador.Apellidos}" : "",
                CalificadoId = c.CalificadoId,
                Puntuacion = c.Puntuacion,
                Comentario = c.Comentario,
                CreadoEn = c.CreatedAt
            });
        }

        return items;
    }
}