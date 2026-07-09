using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Notifications.Commands.MarcarLeida;
using ExchangePlatform.Application.Features.Notifications.Commands.MarcarTodasLeidas;
using ExchangePlatform.Application.Features.Notifications.Queries.ObtenerNotificaciones;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerNotificaciones()
    {
        var usuarioId = User.GetUserId();
        var notificaciones = await _mediator.Send(
            new ObtenerNotificacionesQuery(usuarioId));
        return Ok(ApiResponse<object>.Ok(notificaciones));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarcarLeida(Guid id)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new MarcarLeidaCommand(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(null, "Notificación marcada como leída."));
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new MarcarTodasLeidasCommand(usuarioId));
        return Ok(ApiResponse<object>.Ok(null, "Todas las notificaciones marcadas como leídas."));
    }
}