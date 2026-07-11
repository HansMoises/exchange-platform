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

        // Detrás del pooler de Supabase (Supavisor) hay DOS pools compitiendo: el
        // del lado cliente (Npgsql) reutiliza conexiones que el pooler ya recicló
        // o cerró silenciosamente. Esa conexión queda "medio abierta": la primera
        // query va bien (~50ms) y la siguiente, en la MISMA conexión, se cuelga en
        // la lectura del socket hasta el CommandTimeout, lanzando
        //   "Npgsql.NpgsqlException: Exception while reading from stream"
        //   ---> "System.TimeoutException: Timeout during reading attempt".
        // No es un problema de inactividad (la conexión muere entre dos peticiones
        // seguidas), así que ConnectionIdleLifetime no lo resuelve.
        //
        // Patrón correcto "app detrás de un pooler": desactivar el pool del cliente
        // (Pooling=false) y dejar que Supabase gestione las conexiones. Cada
        // petición abre una conexión fresca al pooler (coste bajo) y nunca reutiliza
        // una conexión muerta. KeepAlive protege operaciones largas dentro de una
        // misma conexión; Timeout/CommandTimeout acotan la espera.
        // NpgsqlConnectionStringBuilder respeta lo que ya venga en la cadena
        // (host/usuario/password del ambiente) y solo añade estos ajustes.
        var csb = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Pooling = false,
            TcpKeepAlive = true,
            KeepAlive = 15,
            Timeout = 15,
            CommandTimeout = 30,
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