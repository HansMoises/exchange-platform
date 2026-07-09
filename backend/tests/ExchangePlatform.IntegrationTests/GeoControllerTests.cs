using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.IntegrationTests;

// GeoController no requiere autenticacion (datos publicos de referencia).
// Departamentos/Provincias/Distritos no tienen seed data via migraciones
// (a diferencia de Categorias, que si la tiene), por lo que insertamos filas
// directo en la BD para poder probar el caso con datos.
public class GeoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public GeoControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    // Idempotente: varios metodos de prueba comparten la misma BD dentro del
    // mismo IClassFixture, y Ubigeo tiene indice unico, asi que reutilizamos
    // las filas si ya existen en vez de insertarlas de nuevo.
    private async Task<(int DepartamentoId, int ProvinciaId, int DistritoId)> SembrarUbicacionAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();

        var departamentoId = await db.Departamentos
            .Where(d => d.Nombre == "Ayacucho").Select(d => d.Id).FirstOrDefaultAsync();
        if (departamentoId == 0)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO \"Departamentos\" (\"Ubigeo\", \"Nombre\") VALUES ('05', 'Ayacucho')");
            departamentoId = await db.Departamentos
                .OrderByDescending(d => d.Id).Select(d => d.Id).FirstAsync();
        }

        var provinciaId = await db.Provincias
            .Where(p => p.Nombre == "Huamanga" && p.DepartamentoId == departamentoId)
            .Select(p => p.Id).FirstOrDefaultAsync();
        if (provinciaId == 0)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO \"Provincias\" (\"Ubigeo\", \"Nombre\", \"DepartamentoId\") VALUES ('0501', 'Huamanga', {departamentoId})");
            provinciaId = await db.Provincias
                .OrderByDescending(p => p.Id).Select(p => p.Id).FirstAsync();
        }

        var distritoId = await db.Distritos
            .Where(d => d.Nombre == "Ayacucho" && d.ProvinciaId == provinciaId)
            .Select(d => d.Id).FirstOrDefaultAsync();
        if (distritoId == 0)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO \"Distritos\" (\"Ubigeo\", \"Nombre\", \"ProvinciaId\") VALUES ('050101', 'Ayacucho', {provinciaId})");
            distritoId = await db.Distritos
                .OrderByDescending(d => d.Id).Select(d => d.Id).FirstAsync();
        }

        return (departamentoId, provinciaId, distritoId);
    }

    [Fact]
    public async Task GetDepartamentos_devuelve_los_departamentos_existentes()
    {
        var (departamentoId, _, _) = await SembrarUbicacionAsync();
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/v1/geo/departamentos");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should()
            .ContainSingle(d => d.GetProperty("id").GetInt32() == departamentoId
                && d.GetProperty("nombre").GetString() == "Ayacucho");
    }

    [Fact]
    public async Task GetProvincias_filtra_por_departamento()
    {
        var (departamentoId, provinciaId, _) = await SembrarUbicacionAsync();
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync($"/api/v1/geo/provincias?departamentoId={departamentoId}");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should()
            .ContainSingle(p => p.GetProperty("id").GetInt32() == provinciaId
                && p.GetProperty("nombre").GetString() == "Huamanga");
    }

    [Fact]
    public async Task GetProvincias_sin_coincidencias_devuelve_lista_vacia()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/v1/geo/provincias?departamentoId=999999");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should().BeEmpty();
    }

    [Fact]
    public async Task GetDistritos_filtra_por_provincia()
    {
        var (_, provinciaId, distritoId) = await SembrarUbicacionAsync();
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync($"/api/v1/geo/distritos?provinciaId={provinciaId}");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should()
            .ContainSingle(d => d.GetProperty("id").GetInt32() == distritoId
                && d.GetProperty("nombre").GetString() == "Ayacucho");
    }

    [Fact]
    public async Task GetCategorias_devuelve_las_categorias_activas_sembradas_por_migracion()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/v1/geo/categorias");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should()
            .Contain(c => c.GetProperty("nombre").GetString() == "Electrónica");
    }
}
