using MediatR;

namespace ExchangePlatform.Application.Features.Reports.Commands.CrearReporte;

public record CrearReporteCommand(
    Guid ReportanteId,
    string EntidadTipo,
    Guid EntidadId,
    string Motivo,
    string? Descripcion) : IRequest<Guid>;