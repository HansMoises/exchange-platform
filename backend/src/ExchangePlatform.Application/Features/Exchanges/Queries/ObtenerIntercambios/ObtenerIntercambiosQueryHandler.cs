using ExchangePlatform.Application.Features.Exchanges.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Queries.ObtenerIntercambios;

public class ObtenerIntercambiosQueryHandler
    : IRequestHandler<ObtenerIntercambiosQuery, List<IntercambioDto>>
{
    private readonly IUnitOfWork _uow;

    public ObtenerIntercambiosQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<IntercambioDto>> Handle(
        ObtenerIntercambiosQuery request, CancellationToken ct)
    {
        var intercambios = await _uow.Intercambios
            .GetByUsuarioIdAsync(request.UsuarioId,
                request.PageNumber, request.PageSize);

        var items = new List<IntercambioDto>();

        foreach (var i in intercambios)
        {
            var objSolicitado = await _uow.Objetos.GetByIdAsync(i.ObjetoSolicitadoId);
            var objOfrecido = await _uow.Objetos.GetByIdAsync(i.ObjetoOfrecidoId);
            var solicitante = await _uow.Usuarios.GetByIdAsync(i.UsuarioSolicitanteId);
            var propietario = await _uow.Usuarios.GetByIdAsync(i.UsuarioPropietarioId);

            items.Add(new IntercambioDto
            {
                Id = i.Id,
                ObjetoSolicitadoId = i.ObjetoSolicitadoId,
                ObjetoSolicitadoTitulo = objSolicitado?.Titulo ?? "",
                ObjetoOfrecidoId = i.ObjetoOfrecidoId,
                ObjetoOfrecidoTitulo = objOfrecido?.Titulo ?? "",
                UsuarioSolicitanteId = i.UsuarioSolicitanteId,
                UsuarioSolicitanteNombres = solicitante != null
                    ? $"{solicitante.Nombres} {solicitante.Apellidos}" : "",
                UsuarioPropietarioId = i.UsuarioPropietarioId,
                UsuarioPropietarioNombres = propietario != null
                    ? $"{propietario.Nombres} {propietario.Apellidos}" : "",
                Estado = i.Estado.ToString(),
                MensajeInicial = i.MensajeInicial,
                ConfirmacionSolicitante = i.ConfirmacionSolicitante,
                ConfirmacionPropietario = i.ConfirmacionPropietario,
                FechaAceptacion = i.FechaAceptacion,
                FechaCompletado = i.FechaCompletado,
                CreadoEn = i.CreatedAt
            });
        }

        return items;
    }
}