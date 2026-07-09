using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Repositories;
using ExchangePlatform.Infrastructure.Persistence.Repositories;

namespace ExchangePlatform.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ExchangePlatformDbContext _context;

    public IUsuarioRepository Usuarios { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IPasswordResetTokenRepository PasswordResetTokens { get; }
    public IObjetoRepository Objetos { get; }
    public IIntercambioRepository Intercambios { get; }
    public ICategoriaRepository Categorias { get; }
    public ICalificacionRepository Calificaciones { get; }
    public IFavoritoRepository Favoritos { get; }
    public IReporteRepository Reportes { get; }
    public INotificacionRepository Notificaciones { get; }
    public IAuditLogRepository AuditLogs { get; }

    public UnitOfWork(ExchangePlatformDbContext context)
    {
        _context = context;
        Usuarios = new UsuarioRepository(context);
        RefreshTokens = new RefreshTokenRepository(context);
        PasswordResetTokens = new PasswordResetTokenRepository(context);
        Objetos = new ObjetoRepository(context);
        Intercambios = new IntercambioRepository(context);
        Categorias = new CategoriaRepository(context);
        Calificaciones = new CalificacionRepository(context);
        Favoritos = new FavoritoRepository(context);
        Reportes = new ReporteRepository(context);
        Notificaciones = new NotificacionRepository(context);
        AuditLogs = new AuditLogRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}