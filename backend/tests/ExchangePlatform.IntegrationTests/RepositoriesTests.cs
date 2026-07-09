using ExchangePlatform.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.IntegrationTests;

// Cubre métodos de repositorio que ningún endpoint ejercita directamente,
// invocándolos contra el DbContext real (Testcontainers PostgreSQL).
public class RepositoriesTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RepositoriesTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Metodos_de_repositorio_no_cubiertos_por_endpoints_se_ejecutan()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        (await uow.Reportes.GetPendientesAsync()).Should().NotBeNull();
        (await uow.Calificaciones.GetByIntercambioIdAsync(Guid.NewGuid())).Should().NotBeNull();
        (await uow.Objetos.GetByUsuarioIdAsync(Guid.NewGuid())).Should().NotBeNull();
        (await uow.Objetos.GetDisponiblesByUsuarioIdAsync(Guid.NewGuid())).Should().NotBeNull();

        await uow.AuditLogs.RegistrarAsync(
            Guid.NewGuid(), "PruebaAuditoria", "Test", "e1", "Ok", "127.0.0.1", "detalle de prueba");
    }
}
