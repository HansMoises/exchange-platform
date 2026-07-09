using ExchangePlatform.API.Extensions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Entities.Maestras;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Administrador,Moderador")]
public class AdminController : ControllerBase
{
    private readonly ExchangePlatformDbContext _context;

    public AdminController(ExchangePlatformDbContext context)
    {
        _context = context;
    }

    // GET /admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var totalUsuarios = await _context.Usuarios.CountAsync();
        var totalObjetos = await _context.Objetos.CountAsync();
        var intercambiosCompletados = await _context.Intercambios
            .CountAsync(i => i.Estado == EstadoIntercambio.Completado);
        var intercambiosPendientes = await _context.Intercambios
            .CountAsync(i => i.Estado == EstadoIntercambio.Pendiente);
        var reportesPendientes = await _context.Reportes
            .CountAsync(r => r.EstadoReporte == EstadoReporte.Pendiente);
        var hace30dias = DateTime.UtcNow.AddDays(-30);
        var usuariosActivos = await _context.Usuarios
            .CountAsync(u => u.CreatedAt >= hace30dias);

        return Ok(ApiResponse<object>.Ok(new
        {
            totalUsuarios,
            totalObjetos,
            intercambiosCompletados,
            intercambiosPendientes,
            reportesPendientes,
            usuariosActivos30d = usuariosActivos
        }));
    }

    // GET /admin/users
    [HttpGet("users")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetUsuarios(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Usuarios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Nombres.Contains(search) ||
                u.Apellidos.Contains(search) ||
                u.Email.Contains(search));

        var total = await query.CountAsync();
        var usuarios = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Nombres,
                u.Apellidos,
                u.Email,
                u.RolId,
                u.IsActive,
                u.CalificacionPromedio,
                u.TotalIntercambios,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(
            PagedResult<object>.Create(
                usuarios.Cast<object>().ToList(),
                pageNumber, pageSize, total)));
    }

    // PATCH /admin/users/{id}/activate
    [HttpPatch("users/{id:guid}/activate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActivarUsuario(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));

        usuario.Activar();
        usuario.UpdatedBy = User.GetUserId();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Usuario activado."));
    }

    // PATCH /admin/users/{id}/deactivate
    [HttpPatch("users/{id:guid}/deactivate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarUsuario(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));

        usuario.Desactivar();
        usuario.UpdatedBy = User.GetUserId();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Usuario desactivado."));
    }

    // DELETE /admin/users/{id}
    [HttpDelete("users/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> EliminarUsuario(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));

        // RN-013: eliminacion logica (soft delete), no fisica.
        usuario.MarcarEliminado(User.GetUserId());
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Usuario eliminado."));
    }

    // PATCH /admin/users/{id}/role
    [HttpPatch("users/{id:guid}/role")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarRolUsuario(Guid id, [FromBody] CambiarRolRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));

        var existeRol = await _context.Roles.AnyAsync(r => r.Id == request.RolId);
        if (!existeRol)
            return UnprocessableEntity(ApiResponse<object>.Fail("El rol no es valido."));

        usuario.CambiarRol(request.RolId);
        usuario.UpdatedBy = User.GetUserId();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Rol actualizado."));
    }

    // GET /admin/objects
    [HttpGet("objects")]
    public async Task<IActionResult> GetObjetos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var total = await _context.Objetos.CountAsync();
        var objetos = await _context.Objetos
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.Titulo,
                o.Estado,
                o.CategoriaId,
                o.UsuarioId,
                o.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(
            PagedResult<object>.Create(
                objetos.Cast<object>().ToList(),
                pageNumber, pageSize, total)));
    }

    // PATCH /admin/objects/{id}/suspend
    [HttpPatch("objects/{id:guid}/suspend")]
    public async Task<IActionResult> SuspenderObjeto(Guid id)
    {
        var objeto = await _context.Objetos.FindAsync(id);
        if (objeto == null)
            return NotFound(ApiResponse<object>.Fail("Objeto no encontrado."));

        objeto.Suspender();
        objeto.UpdatedBy = User.GetUserId();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Objeto suspendido."));
    }

    // PATCH /admin/objects/{id}/restore
    [HttpPatch("objects/{id:guid}/restore")]
    public async Task<IActionResult> RestaurarObjeto(Guid id)
    {
        var objeto = await _context.Objetos.FindAsync(id);
        if (objeto == null)
            return NotFound(ApiResponse<object>.Fail("Objeto no encontrado."));

        objeto.Restaurar();
        objeto.UpdatedBy = User.GetUserId();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Objeto restaurado."));
    }

    // GET /admin/reports
    [HttpGet("reports")]
    public async Task<IActionResult> GetReportes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var total = await _context.Reportes.CountAsync();
        var reportes = await _context.Reportes
            .OrderBy(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.ReportanteId,
                r.EntidadTipo,
                r.EntidadId,
                r.Motivo,
                r.Descripcion,
                r.EstadoReporte,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(
            PagedResult<object>.Create(
                reportes.Cast<object>().ToList(),
                pageNumber, pageSize, total)));
    }

    // PATCH /admin/reports/{id}/resolve
    [HttpPatch("reports/{id:guid}/resolve")]
    public async Task<IActionResult> ResolverReporte(Guid id)
    {
        var reporte = await _context.Reportes.FindAsync(id);
        if (reporte == null)
            return NotFound(ApiResponse<object>.Fail("Reporte no encontrado."));

        reporte.Resolver(User.GetUserId());
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Reporte resuelto."));
    }

    // PATCH /admin/reports/{id}/discard
    [HttpPatch("reports/{id:guid}/discard")]
    public async Task<IActionResult> DescartarReporte(Guid id)
    {
        var reporte = await _context.Reportes.FindAsync(id);
        if (reporte == null)
            return NotFound(ApiResponse<object>.Fail("Reporte no encontrado."));

        reporte.Descartar(User.GetUserId());
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Reporte descartado."));
    }

    // GET /admin/audit-logs
    [HttpGet("audit-logs")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var total = await _context.AuditLogs.CountAsync();
        var logs = await _context.AuditLogs
            .OrderByDescending(a => a.OcurridoEn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.UsuarioId,
                UsuarioNombres = _context.Usuarios
                    .Where(u => u.Id == a.UsuarioId)
                    .Select(u => u.Nombres + " " + u.Apellidos)
                    .FirstOrDefault(),
                UsuarioEmail = _context.Usuarios
                    .Where(u => u.Id == a.UsuarioId)
                    .Select(u => u.Email)
                    .FirstOrDefault(),
                a.Accion,
                a.EntidadTipo,
                a.EntidadId,
                a.Resultado,
                a.IpAddress,
                a.OcurridoEn
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(
            PagedResult<object>.Create(
                logs.Cast<object>().ToList(),
                pageNumber, pageSize, total)));
    }

    // GET /admin/categories
    [HttpGet("categories")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _context.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Descripcion,
                c.Icono,
                c.IsActive
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(categorias));
    }

    // POST /admin/categories
    [HttpPost("categories")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearCategoria([FromBody] CategoriaRequest request)
    {
        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Nombre == request.Nombre);
        if (existeNombre)
            return UnprocessableEntity(ApiResponse<object>.Fail("Ya existe una categoría con ese nombre."));

        var categoria = new Categoria(request.Nombre, request.Descripcion, request.Icono);
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return Created($"/api/v1/admin/categories/{categoria.Id}",
            ApiResponse<object>.Ok(new { categoria.Id }, "Categoría creada."));
    }

    // PUT /admin/categories/{id}
    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] CategoriaRequest request)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound(ApiResponse<object>.Fail("Categoría no encontrada."));

        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Nombre == request.Nombre && c.Id != id);
        if (existeNombre)
            return UnprocessableEntity(ApiResponse<object>.Fail("Ya existe una categoría con ese nombre."));

        categoria.Actualizar(request.Nombre, request.Descripcion, request.Icono);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Categoría actualizada."));
    }

    // PATCH /admin/categories/{id}/activate
    [HttpPatch("categories/{id:int}/activate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActivarCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound(ApiResponse<object>.Fail("Categoría no encontrada."));

        categoria.Activar();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Categoría activada."));
    }

    // PATCH /admin/categories/{id}/deactivate
    [HttpPatch("categories/{id:int}/deactivate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound(ApiResponse<object>.Fail("Categoría no encontrada."));

        categoria.Desactivar();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Categoría desactivada."));
    }
}

public record CambiarRolRequest(int RolId);
public record CategoriaRequest(string Nombre, string? Descripcion, string? Icono);