using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Ratings.Commands.CrearCalificacion;
using ExchangePlatform.Application.Features.Ratings.Queries.ObtenerCalificaciones;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/ratings")]
public class RatingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RatingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear([FromBody] CrearCalificacionRequest request)
    {
        var usuarioId = User.GetUserId();
        var id = await _mediator.Send(new CrearCalificacionCommand(
            request.IntercambioId,
            usuarioId,
            request.Puntuacion,
            request.Comentario));

        return Created($"/api/v1/ratings/{id}",
            ApiResponse<object>.Ok(new { id },
                "Calificación registrada exitosamente."));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> ObtenerPorUsuario(Guid userId)
    {
        var calificaciones = await _mediator.Send(
            new ObtenerCalificacionesQuery(userId));
        return Ok(ApiResponse<object>.Ok(calificaciones));
    }
}

public record CrearCalificacionRequest(
    Guid IntercambioId,
    int Puntuacion,
    string? Comentario);