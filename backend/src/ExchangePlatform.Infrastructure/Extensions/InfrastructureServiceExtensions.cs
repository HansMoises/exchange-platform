using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Infrastructure.Persistence;
using ExchangePlatform.Infrastructure.Persistence.Repositories;
using ExchangePlatform.Infrastructure.Security;
using ExchangePlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Motor de base de datos: PostgreSQL (Supabase en producción, ver ADR-010).
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ExchangePlatformDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(typeof(ExchangePlatformDbContext).Assembly.FullName));
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