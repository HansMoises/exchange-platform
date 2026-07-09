using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.CrearIntercambio;

public class CrearIntercambioCommandHandler
    : IRequestHandler<CrearIntercambioCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly INotificacionService _notificacionService;

    public CrearIntercambioCommandHandler(
        IUnitOfWork uow, INotificacionService notificacionService)
    {
        _uow = uow;
        _notificacionService = notificacionService;
    }

    public async Task<Guid> Handle(
        CrearIntercambioCommand request, CancellationToken ct)
    {
        var objetoSolicitado = await _uow.Objetos
            .GetByIdAsync(request.ObjetoSolicitadoId)
            ?? throw new NotFoundException("Objeto solicitado no encontrado.");

        if (objetoSolicitado.UsuarioId == request.UsuarioSolicitanteId)
            throw new ConflictException("No puedes solicitar tu propio objeto.");

        if (objetoSolicitado.Estado != EstadoObjeto.Disponible)
            throw new ConflictException("El objeto no está disponible.");

        var objetoOfrecido = await _uow.Objetos
            .GetByIdAsync(request.ObjetoOfrecidoId)
            ?? throw new NotFoundException("Objeto ofrecido no encontrado.");

        if (objetoOfrecido.UsuarioId != request.UsuarioSolicitanteId)
            throw new ForbiddenException("El objeto ofrecido debe ser tuyo.");

        if (objetoOfrecido.Estado != EstadoObjeto.Disponible)
            throw new ConflictException("Tu objeto ofrecido no está disponible.");

        var tieneSolicitudActiva = await _uow.Intercambios
            .TieneSolicitudActivaAsync(
                request.UsuarioSolicitanteId,
                request.ObjetoSolicitadoId);

        if (tieneSolicitudActiva)
            throw new ConflictException(
                "Ya tienes una solicitud activa para este objeto.");

        var intercambio = new Intercambio(
            request.ObjetoSolicitadoId,
            request.ObjetoOfrecidoId,
            request.UsuarioSolicitanteId,
            objetoSolicitado.UsuarioId,
            request.MensajeInicial);

        intercambio.CreatedBy = request.UsuarioSolicitanteId;
        await _uow.Intercambios.AddAsync(intercambio);
        await _uow.SaveChangesAsync(ct);

        // Notificar al propietario
        var solicitante = await _uow.Usuarios
            .GetByIdAsync(request.UsuarioSolicitanteId);
        var nombreSolicitante = solicitante != null
            ? $"{solicitante.Nombres} {solicitante.Apellidos}" : "Un usuario";

        await _notificacionService.CrearAsync(
            objetoSolicitado.UsuarioId,
            TipoNotificacion.SolicitudRecibida,
            "Nueva solicitud de intercambio",
            $"{nombreSolicitante} quiere intercambiar por tu objeto '{objetoSolicitado.Titulo}'.",
            "Intercambio",
            intercambio.Id);

        return intercambio.Id;
    }
}