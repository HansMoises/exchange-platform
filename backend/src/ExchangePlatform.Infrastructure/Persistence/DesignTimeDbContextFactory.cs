using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExchangePlatform.Infrastructure.Persistence;

// Fabrica usada SOLO por las herramientas de EF Core (dotnet ef) en tiempo de diseno.
// Motor unico: PostgreSQL (Supabase, ver ADR-010). No afecta la ejecucion normal
// de la aplicacion (esa usa InfrastructureServiceExtensions). Se excluye de la
// cobertura por ser codigo de tooling que nunca se ejecuta en pruebas ni en runtime.
[ExcludeFromCodeCoverage]
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ExchangePlatformDbContext>
{
    public ExchangePlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=exchange_platform;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ExchangePlatformDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("ExchangePlatform.Infrastructure"));

        return new ExchangePlatformDbContext(optionsBuilder.Options);
    }
}
