using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Entities.Geo;
using ExchangePlatform.Domain.Entities.Maestras;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence;

public class ExchangePlatformDbContext : DbContext
{
    public ExchangePlatformDbContext(DbContextOptions<ExchangePlatformDbContext> options)
        : base(options) { }

    // Maestras
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Distrito> Distritos => Set<Distrito>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Categoria> Categorias => Set<Categoria>();

    // Seguridad
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    // Transaccionales
    public DbSet<Objeto> Objetos => Set<Objeto>();
    public DbSet<ImagenObjeto> ImagenesObjeto => Set<ImagenObjeto>();
    public DbSet<Intercambio> Intercambios => Set<Intercambio>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<Reporte> Reportes => Set<Reporte>();

    // Auditoría
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ExchangePlatformDbContext).Assembly);

        // Filtros globales Soft Delete (RN-013)
        modelBuilder.Entity<Usuario>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Objeto>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Intercambio>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Reporte>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Favorito>().HasQueryFilter(e => !e.IsDeleted);

        // Índices parciales (filtered) para no bloquear duplicados de filas ya
        // borradas (soft delete). Sintaxis PostgreSQL (comillas dobles, ADR-010).
        const string deletedFilterSql = "\"IsDeleted\" = false";

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .HasFilter(deletedFilterSql);

        modelBuilder.Entity<Favorito>()
            .HasIndex(f => new { f.UsuarioId, f.ObjetoId })
            .HasFilter(deletedFilterSql);

        modelBuilder.Entity<Intercambio>()
            .HasIndex(i => i.UsuarioSolicitanteId)
            .HasFilter(deletedFilterSql);

        modelBuilder.Entity<Intercambio>()
            .HasIndex(i => i.UsuarioPropietarioId)
            .HasFilter(deletedFilterSql);

        modelBuilder.Entity<Objeto>()
            .HasIndex(o => o.Estado)
            .HasFilter(deletedFilterSql);

        modelBuilder.Entity<Objeto>()
            .HasIndex(o => o.CategoriaId)
            .HasFilter(deletedFilterSql);
    }
}