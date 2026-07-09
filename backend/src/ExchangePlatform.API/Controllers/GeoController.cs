using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/geo")]
public class GeoController : ControllerBase
{
    private readonly GeoRepository _geoRepository;

    public GeoController(GeoRepository geoRepository)
    {
        _geoRepository = geoRepository;
    }

    [HttpGet("departamentos")]
    public async Task<IActionResult> GetDepartamentos()
    {
        var departamentos = await _geoRepository.GetDepartamentosAsync();
        var data = departamentos.Select(d => new
        {
            d.Id,
            d.Ubigeo,
            d.Nombre
        });
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("provincias")]
    public async Task<IActionResult> GetProvincias([FromQuery] int departamentoId)
    {
        var provincias = await _geoRepository
            .GetProvinciasByDepartamentoAsync(departamentoId);
        var data = provincias.Select(p => new
        {
            p.Id,
            p.Ubigeo,
            p.Nombre,
            p.DepartamentoId
        });
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("distritos")]
    public async Task<IActionResult> GetDistritos([FromQuery] int provinciaId)
    {
        var distritos = await _geoRepository
            .GetDistritosByProvinciaAsync(provinciaId);
        var data = distritos.Select(d => new
        {
            d.Id,
            d.Ubigeo,
            d.Nombre,
            d.ProvinciaId
        });
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _geoRepository.GetCategoriasActivasAsync();
        var data = categorias.Select(c => new
        {
            c.Id,
            c.Nombre,
            c.Descripcion,
            c.Icono
        });
        return Ok(ApiResponse<object>.Ok(data));
    }
}