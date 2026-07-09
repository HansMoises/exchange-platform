using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Application.Features.Exchanges.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Queries.ObtenerIntercambioPorId;

public class ObtenerIntercambioPorIdQueryHandler
    : IRequestHandler<ObtenerIntercambioPorIdQuery, IntercambioDto>
{
    private readonly IUnitOfWork _uow;

    public ObtenerIntercambioPorIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IntercambioDto> Handle(
        ObtenerIntercambioPorIdQuery request, CancellationToken ct)
    {
        var i = await _uow.Intercambios.GetByIdAsync(request.IntercambioId)
            ?? throw new NotFoundException("Intercambio no encontrado.");

        if (i.UsuarioSolicitanteId != request.UsuarioId &&
            i.UsuarioPropietarioId != request.UsuarioId)
            throw new ForbiddenException("No tienes acceso a este intercambio.");

        var objSolicitado = await _uow.Objetos.GetByIdAsync(i.ObjetoSolicitadoId);
        var objOfrecido = await _uow.Objetos.GetByIdAsync(i.ObjetoOfrecidoId);
        var solicitante = await _uow.Usuarios.GetByIdAsync(i.UsuarioSolicitanteId);
        var propietario = await _uow.Usuarios.GetByIdAsync(i.UsuarioPropietarioId);

        return new IntercambioDto
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
        };
    }
}