using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Favorites.Commands.AgregarFavorito;
using ExchangePlatform.Application.Features.Favorites.Commands.QuitarFavorito;
using ExchangePlatform.Application.Features.Favorites.Queries.ObtenerFavoritos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FavoritesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerFavoritos()
    {
        var usuarioId = User.GetUserId();
        var favoritos = await _mediator.Send(
            new ObtenerFavoritosQuery(usuarioId));
        return Ok(ApiResponse<object>.Ok(favoritos));
    }

    [HttpPost]
    public async Task<IActionResult> Agregar([FromBody] AgregarFavoritoRequest request)
    {
        var usuarioId = User.GetUserId();
        var id = await _mediator.Send(
            new AgregarFavoritoCommand(usuarioId, request.ObjetoId));
        return Created($"/api/v1/favorites/{id}",
            ApiResponse<object>.Ok(new { id }, "Agregado a favoritos."));
    }

    [HttpDelete("{objetoId:guid}")]
    public async Task<IActionResult> Quitar(Guid objetoId)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new QuitarFavoritoCommand(usuarioId, objetoId));
        return Ok(ApiResponse<object>.Ok(null, "Eliminado de favoritos."));
    }
}

public record AgregarFavoritoRequest(Guid ObjetoId);