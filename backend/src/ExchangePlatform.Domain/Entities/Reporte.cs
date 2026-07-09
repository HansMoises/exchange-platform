using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Exceptions;

namespace ExchangePlatform.Domain.Entities;

public class Reporte : BaseEntity
{
    public Guid ReportanteId { get; private set; }
    public string EntidadTipo { get; private set; } = string.Empty;
    public Guid EntidadId { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public EstadoReporte EstadoReporte { get; private set; } = EstadoReporte.Pendiente;
    public Guid? ResueltoPorId { get; private set; }
    public DateTime? FechaResolucion { get; private set; }

    // EF Core
    protected Reporte() { }

    public Reporte(Guid reportanteId, string entidadTipo,
                   Guid entidadId, string motivo, string? descripcion)
    {
        ReportanteId = reportanteId;
        EntidadTipo = entidadTipo;
        EntidadId = entidadId;
        Motivo = motivo;
        Descripcion = descripcion;
    }

    public void Resolver(Guid moderadorId)
    {
        if (EstadoReporte == EstadoReporte.Resuelto)
            throw new DomainException("El reporte ya fue resuelto.");
        EstadoReporte = EstadoReporte.Resuelto;
        ResueltoPorId = moderadorId;
        FechaResolucion = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PasoARevision()
    {
        EstadoReporte = EstadoReporte.EnRevision;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Descartar(Guid moderadorId)
    {
        EstadoReporte = EstadoReporte.Descartado;
        ResueltoPorId = moderadorId;
        FechaResolucion = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}