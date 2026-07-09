using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace ExchangePlatform.IntegrationTests;

// Doble de prueba de IEmailService: captura el ultimo correo "enviado" en
// vez de solo loguearlo (LogEmailService), para poder extraer el token real
// de recuperacion de contrasena en AuthFlowsTests sin leer archivos de log.
public class CapturingEmailService : IEmailService
{
    public string? UltimoDestinatario { get; private set; }
    public string? UltimoAsunto { get; private set; }
    public string? UltimoCuerpo { get; private set; }

    public Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        UltimoDestinatario = destinatario;
        UltimoAsunto = asunto;
        UltimoCuerpo = cuerpo;
        return Task.CompletedTask;
    }
}

// Levanta un PostgreSQL real en Docker (Testcontainers) — el MISMO motor que
// Supabase en produccion (ADR-010) — y aplica las migraciones reales del
// proyecto, en vez de EF InMemory. Asi las pruebas validan restricciones,
// indices parciales y comportamiento identicos a produccion.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Jwt:Secret"] = "IntegrationTestsSecretKeyMustBeAtLeast32CharsLong!",
                ["Jwt:Issuer"] = "exchange-platform",
                ["Jwt:Audience"] = "exchange-platform-users",
                ["Jwt:AccessTokenExpiryMinutes"] = "15",
                ["Jwt:RefreshTokenExpiryDays"] = "7",
                ["AllowedOrigins"] = "http://localhost:5173",
                ["FrontendUrl"] = "http://localhost:5173",
                ["BackendUrl"] = "https://localhost:7149",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService, CapturingEmailService>();

            // Re-registra el DbContext explicitamente contra el contenedor de
            // prueba. Registro unico y limpio (quita el de AddInfrastructure)
            // para evitar que el proveedor de servicios interno de EF, cacheado
            // al arrancar el host de pruebas, aplique las migraciones dos veces.
            services.RemoveAll<DbContextOptions<ExchangePlatformDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<ExchangePlatformDbContext>(opt =>
                opt.UseNpgsql(_dbContainer.GetConnectionString(),
                    b => b.MigrationsAssembly(typeof(ExchangePlatformDbContext).Assembly.FullName)));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
