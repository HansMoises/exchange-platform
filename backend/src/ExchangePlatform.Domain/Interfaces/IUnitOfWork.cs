using ExchangePlatform.Domain.Interfaces.Repositories;

namespace ExchangePlatform.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAuditLogRepository AuditLogs { get; }
    INotificacionRepository Notificaciones { get; }
    IReporteRepository Reportes { get; }
    IFavoritoRepository Favoritos { get; }
    ICalificacionRepository Calificaciones { get; }
    IUsuarioRepository Usuarios { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IPasswordResetTokenRepository PasswordResetTokens { get; }
    IObjetoRepository Objetos { get; }
    IIntercambioRepository Intercambios { get; }
    ICategoriaRepository Categorias { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}