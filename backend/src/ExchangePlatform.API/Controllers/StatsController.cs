using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/stats")]
public class StatsController : ControllerBase
{
    private readonly ExchangePlatformDbContext _context;

    public StatsController(ExchangePlatformDbContext context)
    {
        _context = context;
    }

    // GET /stats/public
    // Solo agregados no sensibles, pensado para la landing page publica.
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicos()
    {
        var totalUsuarios = await _context.Usuarios.CountAsync(u => u.IsActive);
        var totalObjetos = await _context.Objetos.CountAsync(o => o.Estado == EstadoObjeto.Disponible);
        var intercambiosCompletados = await _context.Intercambios
            .CountAsync(i => i.Estado == EstadoIntercambio.Completado);

        return Ok(ApiResponse<object>.Ok(new
        {
            totalUsuarios,
            totalObjetos,
            intercambiosCompletados
        }));
    }
}
