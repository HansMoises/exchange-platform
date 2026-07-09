using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Exchanges.Commands.AceptarIntercambio;
using ExchangePlatform.Application.Features.Exchanges.Commands.CancelarIntercambio;
using ExchangePlatform.Application.Features.Exchanges.Commands.ConfirmarIntercambio;
using ExchangePlatform.Application.Features.Exchanges.Commands.CrearIntercambio;
using ExchangePlatform.Application.Features.Exchanges.Commands.RechazarIntercambio;
using ExchangePlatform.Application.Features.Exchanges.Queries.ObtenerIntercambioPorId;
using ExchangePlatform.Application.Features.Exchanges.Queries.ObtenerIntercambios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/exchanges")]
[Authorize]
public class ExchangesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearIntercambioRequest request)
    {
        var usuarioId = User.GetUserId();
        var id = await _mediator.Send(new CrearIntercambioCommand(
            request.ObjetoSolicitadoId,
            request.ObjetoOfrecidoId,
            usuarioId,
            request.MensajeInicial));

        return Created($"/api/v1/exchanges/{id}",
            ApiResponse<object>.Ok(new { id },
                "Solicitud de intercambio enviada exitosamente."));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMisIntercambios(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var usuarioId = User.GetUserId();
        var intercambios = await _mediator.Send(
            new ObtenerIntercambiosQuery(usuarioId, pageNumber, pageSize));
        return Ok(ApiResponse<object>.Ok(intercambios));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var usuarioId = User.GetUserId();
        var intercambio = await _mediator.Send(
            new ObtenerIntercambioPorIdQuery(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(intercambio));
    }

    [HttpPatch("{id:guid}/accept")]
    public async Task<IActionResult> Aceptar(Guid id)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new AceptarIntercambioCommand(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(null, "Solicitud aceptada exitosamente."));
    }

    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Rechazar(Guid id)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new RechazarIntercambioCommand(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(null, "Solicitud rechazada."));
    }

    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> Confirmar(Guid id)
    {
        var usuarioId = User.GetUserId();
        var resultado = await _mediator.Send(
            new ConfirmarIntercambioCommand(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(resultado, "Confirmación registrada."));
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new CancelarIntercambioCommand(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(null, "Intercambio cancelado."));
    }
}

public record CrearIntercambioRequest(
    Guid ObjetoSolicitadoId,
    Guid ObjetoOfrecidoId,
    string? MensajeInicial);