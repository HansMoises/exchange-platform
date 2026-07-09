using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Objects.Commands.ActualizarObjeto;
using ExchangePlatform.Application.Features.Objects.Commands.CrearObjeto;
using ExchangePlatform.Application.Features.Objects.Commands.EliminarObjeto;
using ExchangePlatform.Application.Features.Objects.Queries.ObtenerMisObjetos;
using ExchangePlatform.Application.Features.Objects.Queries.ObtenerMisObjetosDisponibles;
using ExchangePlatform.Application.Features.Objects.Queries.ObtenerObjetoPorId;
using ExchangePlatform.Application.Features.Objects.Queries.ObtenerObjetos;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/objects")]
public class ObjectsController : ControllerBase
{
    // Limites no especificados de forma exacta en la documentacion (RN-011/012
    // solo piden validar tamano/extension/MIME sin dar cifras); se asumen
    // valores razonables hasta que se defina un valor oficial.
    private static readonly string[] TiposMimePermitidos = { "image/jpeg", "image/png", "image/webp" };
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IMediator _mediator;
    private readonly IAlmacenamientoService _almacenamiento;

    public ObjectsController(IMediator mediator, IAlmacenamientoService almacenamiento)
    {
        _mediator = mediator;
        _almacenamiento = almacenamiento;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerObjetos(
        [FromQuery] string? search,
        [FromQuery] int? categoriaId,
        [FromQuery] int? departamentoId,
        [FromQuery] int? provinciaId,
        [FromQuery] int? distritoId,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var resultado = await _mediator.Send(new ObtenerObjetosQuery(
            search, categoriaId, departamentoId,
            provinciaId, distritoId, sortBy, sortOrder, pageNumber, pageSize));

        return Ok(ApiResponse<object>.Ok(resultado));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var objeto = await _mediator.Send(new ObtenerObjetoPorIdQuery(id));
        return Ok(ApiResponse<object>.Ok(objeto));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> ObtenerMisObjetos()
    {
        var usuarioId = User.GetUserId();
        var objetos = await _mediator.Send(new ObtenerMisObjetosQuery(usuarioId));
        return Ok(ApiResponse<object>.Ok(objetos));
    }

    [HttpGet("me/available")]
    [Authorize]
    public async Task<IActionResult> ObtenerMisObjetosDisponibles()
    {
        var usuarioId = User.GetUserId();
        var objetos = await _mediator.Send(new ObtenerMisObjetosDisponiblesQuery(usuarioId));
        return Ok(ApiResponse<object>.Ok(objetos));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear([FromBody] CrearObjetoRequest request)
    {
        var usuarioId = User.GetUserId();
        var id = await _mediator.Send(new CrearObjetoCommand(
            usuarioId,
            request.Titulo,
            request.Descripcion,
            request.CategoriaId,
            request.CondicionFisica,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId,
            request.ImagenesUrl));

        return Created($"/api/v1/objects/{id}",
            ApiResponse<object>.Ok(new { id }, "Objeto publicado exitosamente."));
    }

    [HttpPost("images")]
    [Authorize]
    [RequestSizeLimit(TamanoMaximoBytes)]
    public async Task<IActionResult> SubirImagen(IFormFile archivo, CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Debes seleccionar un archivo."));

        if (archivo.Length > TamanoMaximoBytes)
            return UnprocessableEntity(ApiResponse<object>.Fail("La imagen supera el tamaño máximo de 5 MB."));

        if (!TiposMimePermitidos.Contains(archivo.ContentType))
            return UnprocessableEntity(ApiResponse<object>.Fail("Formato de imagen no permitido. Usa JPEG, PNG o WEBP."));

        var extension = Path.GetExtension(archivo.FileName);
        await using var stream = archivo.OpenReadStream();
        var url = await _almacenamiento.GuardarAsync(stream, extension, ct);

        return Ok(ApiResponse<object>.Ok(new { url }, "Imagen subida exitosamente."));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Actualizar(
        Guid id, [FromBody] ActualizarObjetoRequest request)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new ActualizarObjetoCommand(
            id, usuarioId,
            request.Titulo,
            request.Descripcion,
            request.CategoriaId,
            request.CondicionFisica,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId));

        return Ok(ApiResponse<object>.Ok(null, "Objeto actualizado exitosamente."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var usuarioId = User.GetUserId();
        await _mediator.Send(new EliminarObjetoCommand(id, usuarioId));
        return Ok(ApiResponse<object>.Ok(null, "Objeto eliminado exitosamente."));
    }
}

// Request models
public record CrearObjetoRequest(
    string Titulo,
    string Descripcion,
    int CategoriaId,
    string CondicionFisica,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId,
    List<string> ImagenesUrl);

public record ActualizarObjetoRequest(
    string Titulo,
    string Descripcion,
    int CategoriaId,
    string CondicionFisica,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId);
