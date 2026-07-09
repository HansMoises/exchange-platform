using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ExchangePlatformDbContext _context;

    public AuditLogRepository(ExchangePlatformDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(Guid? usuarioId, string accion,
        string entidadTipo, string? entidadId,
        string resultado, string ipAddress, string? detalle = null)
    {
        var log = new AuditLog(usuarioId, accion, entidadTipo,
            entidadId, resultado, ipAddress, detalle);

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}