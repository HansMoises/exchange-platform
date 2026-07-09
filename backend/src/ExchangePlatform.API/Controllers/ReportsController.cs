using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Reports.Commands.CrearReporte;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearReporteRequest request)
    {
        var usuarioId = User.GetUserId();
        var id = await _mediator.Send(new CrearReporteCommand(
            usuarioId,
            request.EntidadTipo,
            request.EntidadId,
            request.Motivo,
            request.Descripcion));

        return Created($"/api/v1/reports/{id}",
            ApiResponse<object>.Ok(new { id }, "Reporte enviado exitosamente."));
    }
}

public record CrearReporteRequest(
    string EntidadTipo,
    Guid EntidadId,
    string Motivo,
    string? Descripcion);