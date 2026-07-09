using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Reports.Commands.CrearReporte;

public class CrearReporteCommandHandler
    : IRequestHandler<CrearReporteCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CrearReporteCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(
        CrearReporteCommand request, CancellationToken ct)
    {
        // No duplicar reporte activo
        var existeReporte = await _uow.Reportes
            .ExisteReporteActivoAsync(request.ReportanteId, request.EntidadId);

        if (existeReporte)
            throw new ConflictException(
                "Ya tienes un reporte activo sobre esta entidad.");

        var reporte = new Reporte(
            request.ReportanteId,
            request.EntidadTipo,
            request.EntidadId,
            request.Motivo,
            request.Descripcion);

        reporte.CreatedBy = request.ReportanteId;

        await _uow.Reportes.AddAsync(reporte);
        await _uow.SaveChangesAsync(ct);

        return reporte.Id;
    }
}