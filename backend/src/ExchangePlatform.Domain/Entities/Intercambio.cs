using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Exceptions;

namespace ExchangePlatform.Domain.Entities;

public class Intercambio : BaseEntity
{
    public Guid ObjetoSolicitadoId { get; private set; }
    public Guid ObjetoOfrecidoId { get; private set; }
    public Guid UsuarioSolicitanteId { get; private set; }
    public Guid UsuarioPropietarioId { get; private set; }
    public EstadoIntercambio Estado { get; private set; } = EstadoIntercambio.Pendiente;
    public string? MensajeInicial { get; private set; }
    public bool ConfirmacionSolicitante { get; private set; }
    public bool ConfirmacionPropietario { get; private set; }
    public DateTime? FechaAceptacion { get; private set; }
    public DateTime? FechaCompletado { get; private set; }

    // EF Core
    protected Intercambio() { }

    public Intercambio(Guid objetoSolicitadoId, Guid objetoOfrecidoId,
                       Guid solicitanteId, Guid propietarioId,
                       string? mensajeInicial = null)
    {
        if (solicitanteId == propietarioId)
            throw new DomainException("No puedes intercambiar contigo mismo.");

        if (objetoSolicitadoId == objetoOfrecidoId)
            throw new DomainException("Los objetos del intercambio deben ser distintos.");

        ObjetoSolicitadoId = objetoSolicitadoId;
        ObjetoOfrecidoId = objetoOfrecidoId;
        UsuarioSolicitanteId = solicitanteId;
        UsuarioPropietarioId = propietarioId;
        MensajeInicial = mensajeInicial;
    }

    public void Aceptar()
    {
        if (Estado != EstadoIntercambio.Pendiente)
            throw new DomainException("Solo se puede aceptar una solicitud pendiente.");
        Estado = EstadoIntercambio.Aceptado;
        FechaAceptacion = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rechazar()
    {
        if (Estado != EstadoIntercambio.Pendiente)
            throw new DomainException("Solo se puede rechazar una solicitud pendiente.");
        Estado = EstadoIntercambio.Rechazado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirmar(Guid usuarioId)
    {
        if (Estado != EstadoIntercambio.Aceptado &&
            Estado != EstadoIntercambio.PendienteConfirmacion)
            throw new DomainException("El intercambio no está en estado confirmable.");

        if (usuarioId == UsuarioSolicitanteId)
            ConfirmacionSolicitante = true;
        else if (usuarioId == UsuarioPropietarioId)
            ConfirmacionPropietario = true;
        else
            throw new DomainException("Usuario ajeno al intercambio.");

        Estado = (ConfirmacionSolicitante && ConfirmacionPropietario)
            ? EstadoIntercambio.Completado
            : EstadoIntercambio.PendienteConfirmacion;

        if (Estado == EstadoIntercambio.Completado)
            FechaCompletado = DateTime.UtcNow;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Estado == EstadoIntercambio.Completado ||
            Estado == EstadoIntercambio.Rechazado)
            throw new DomainException("No se puede cancelar un intercambio finalizado.");
        Estado = EstadoIntercambio.Cancelado;
        UpdatedAt = DateTime.UtcNow;
    }
}