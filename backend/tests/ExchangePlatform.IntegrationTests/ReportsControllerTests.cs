using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

public class ReportsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public ReportsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Crear_reporte_valido_devuelve_201()
    {
        var reportante = await ClienteAutenticadoAsync();
        var propietario = await ClienteAutenticadoAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto a reportar valido");

        var respuesta = await reportante.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", objetoId, "Spam", "Descripcion del reporte."));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task No_se_puede_reportar_dos_veces_la_misma_entidad()
    {
        var reportante = await ClienteAutenticadoAsync();
        var propietario = await ClienteAutenticadoAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto reportado dos veces valido");

        var primerReporte = await reportante.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", objetoId, "Spam", null));
        primerReporte.StatusCode.Should().Be(HttpStatusCode.Created);

        var segundoReporte = await reportante.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", objetoId, "Fraude", null));

        segundoReporte.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Motivo_invalido_devuelve_422()
    {
        var reportante = await ClienteAutenticadoAsync();
        var propietario = await ClienteAutenticadoAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto con motivo invalido valido");

        var respuesta = await reportante.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", objetoId, "MotivoQueNoExiste", null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task EntidadTipo_invalido_devuelve_422()
    {
        var reportante = await ClienteAutenticadoAsync();

        var respuesta = await reportante.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("TipoInvalido", Guid.NewGuid(), "Spam", null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Sin_autenticacion_devuelve_401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", Guid.NewGuid(), "Spam", null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
