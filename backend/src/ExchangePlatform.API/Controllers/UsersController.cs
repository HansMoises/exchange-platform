using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Users.Commands.ActualizarFoto;
using ExchangePlatform.Application.Features.Users.Commands.ActualizarPerfil;
using ExchangePlatform.Application.Features.Users.Commands.CambiarPassword;
using ExchangePlatform.Application.Features.Users.Queries.ObtenerPerfil;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    // Mismos limites que ObjectsController.SubirImagen (no especificados de
    // forma exacta en la documentacion).
    private static readonly string[] TiposMimePermitidos = { "image/jpeg", "image/png", "image/webp" };
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IMediator _mediator;
    private readonly IAlmacenamientoService _almacenamiento;

    public UsersController(IMediator mediator, IAlmacenamientoService almacenamiento)
    {
        _mediator = mediator;
        _almacenamiento = almacenamiento;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPerfil(Guid id)
    {
        var perfil = await _mediator.Send(new ObtenerPerfilQuery(id));
        return Ok(ApiResponse<object>.Ok(perfil));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        var usuarioId = User.GetUserId();
        var perfil = await _mediator.Send(new ObtenerPerfilQuery(usuarioId));
        return Ok(ApiResponse<object>.Ok(perfil));
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> ActualizarPerfil(
        [FromBody] ActualizarPerfilRequest request)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new ActualizarPerfilCommand(
            usuarioId,
            request.Nombres,
            request.Apellidos,
            request.Telefono,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId));

        return Ok(ApiResponse<object>.Ok(null, "Perfil actualizado exitosamente."));
    }

    [HttpPatch("me/photo")]
    [Authorize]
    [RequestSizeLimit(TamanoMaximoBytes)]
    public async Task<IActionResult> ActualizarFoto(IFormFile archivo, CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Debes seleccionar un archivo."));

        if (archivo.Length > TamanoMaximoBytes)
            return UnprocessableEntity(ApiResponse<object>.Fail("La imagen supera el tamaño máximo de 5 MB."));

        if (!TiposMimePermitidos.Contains(archivo.ContentType))
            return UnprocessableEntity(ApiResponse<object>.Fail("Formato de imagen no permitido. Usa JPEG, PNG o WEBP."));

        var usuarioId = User.GetUserId();
        var extension = Path.GetExtension(archivo.FileName);
        await using var stream = archivo.OpenReadStream();
        var url = await _almacenamiento.GuardarAsync(stream, extension, ct);

        await _mediator.Send(new ActualizarFotoCommand(usuarioId, url));

        return Ok(ApiResponse<object>.Ok(new { url }, "Foto de perfil actualizada exitosamente."));
    }

    [HttpPatch("me/password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword(
        [FromBody] CambiarPasswordRequest request)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new CambiarPasswordCommand(
            usuarioId,
            request.PasswordActual,
            request.PasswordNuevo,
            request.ConfirmPassword));

        return Ok(ApiResponse<object>.Ok(null, "Contraseña actualizada exitosamente."));
    }
}

// Request models
public record ActualizarPerfilRequest(
    string Nombres,
    string Apellidos,
    string Telefono,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId);

public record CambiarPasswordRequest(
    string PasswordActual,
    string PasswordNuevo,
    string ConfirmPassword);
