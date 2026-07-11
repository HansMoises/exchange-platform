using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Infrastructure.Persistence;
using ExchangePlatform.Infrastructure.Persistence.Repositories;
using ExchangePlatform.Infrastructure.Security;
using ExchangePlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ExchangePlatform.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Motor de base de datos: PostgreSQL (Supabase en producción, ver ADR-010).
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Endurecimiento del pool para entornos gestionados (Supabase/pooler): el
        // servidor cierra conexiones inactivas sin avisar; sin esto, Npgsql
        // reutiliza conexiones muertas y la operación se cuelga hasta el Command
        // Timeout -> 500/timeouts intermitentes (se veía como "200 500 200 500"
        // y luego como respuestas de ~30s cuando el reintento las enmascaraba).
        //   - ConnectionIdleLifetime BAJO (5s): descarta conexiones ociosas antes
        //     de que Supabase las cierre, evitando reutilizar una muerta. Es la
        //     clave del fix; 60s era demasiado alto frente al idle de Supabase.
        //   - KeepAlive/TcpKeepAlive: sondea las conexiones activas y detecta muertas.
        //   - Timeout/CommandTimeout acotados: si aun así se cuelga, falla en ~15s
        //     y el reintento abre una conexión nueva (total de pocos segundos, no 60).
        // NpgsqlConnectionStringBuilder respeta lo que ya venga en la cadena de
        // conexión (host/usuario/password del ambiente) y solo añade estos ajustes.
        var csb = new NpgsqlConnectionStringBuilder(connectionString)
        {
            KeepAlive = 15,
            TcpKeepAlive = true,
            ConnectionIdleLifetime = 5,
            ConnectionPruningInterval = 5,
            Timeout = 15,
            CommandTimeout = 15,
        };

        services.AddDbContext<ExchangePlatformDbContext>(options =>
        {
            options.UseNpgsql(
                csb.ConnectionString,
                b =>
                {
                    b.MigrationsAssembly(typeof(ExchangePlatformDbContext).Assembly.FullName);
                    // Reintenta errores transitorios (conexión caída, timeout) en una
                    // conexión nueva. Seguro: la app no usa transacciones explícitas.
                    b.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(3),
                        errorCodesToAdd: null);
                });
        });

        // Unit Of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Servicios de seguridad
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // Servicios de dominio
        services.AddScoped<INotificacionService, NotificacionService>();
        services.AddScoped<IEmailService, LogEmailService>();
        services.AddSingleton<IUrlSettings, UrlSettings>();

        // Repositorios directos
        services.AddScoped<GeoRepository>();

        return services;
    }
}