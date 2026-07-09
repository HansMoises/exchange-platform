using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

public class StatsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public StatsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetPublicos_no_requiere_autenticacion_y_refleja_usuarios_activos()
    {
        var client = _factory.CreateClient();
        await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);

        var respuesta = await client.GetAsync("/api/v1/stats/public");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("totalUsuarios").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        contenido.Data.GetProperty("totalObjetos").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        contenido.Data.GetProperty("intercambiosCompletados").GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetPublicos_refleja_objetos_disponibles_publicados()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await TestObjectHelper.CrearObjetoAsync(client, "Objeto visible en stats publicas");

        var respuesta = await client.GetAsync("/api/v1/stats/public");

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("totalObjetos").GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }
}
