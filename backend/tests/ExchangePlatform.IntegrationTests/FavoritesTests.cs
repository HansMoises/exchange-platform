using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

public class FavoritesTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public FavoritesTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Agregar_y_listar_favorito()
    {
        var propietario = await ClienteAutenticadoAsync();
        var interesado = await ClienteAutenticadoAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto favorito de prueba");

        var agregar = await interesado.PostAsJsonAsync(
            "/api/v1/favorites", new AgregarFavoritoRequest(objetoId));
        agregar.StatusCode.Should().Be(HttpStatusCode.Created);

        var listado = await interesado.GetAsync("/api/v1/favorites");
        listado.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should()
            .ContainSingle(o => o.GetProperty("id").GetGuid() == objetoId);
    }

    [Fact]
    public async Task No_se_puede_agregar_el_mismo_favorito_dos_veces()
    {
        var propietario = await ClienteAutenticadoAsync();
        var interesado = await ClienteAutenticadoAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto favorito duplicado test");

        var primeraVez = await interesado.PostAsJsonAsync(
            "/api/v1/favorites", new AgregarFavoritoRequest(objetoId));
        primeraVez.StatusCode.Should().Be(HttpStatusCode.Created);

        var segundaVez = await interesado.PostAsJsonAsync(
            "/api/v1/favorites", new AgregarFavoritoRequest(objetoId));
        segundaVez.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Quitar_favorito_lo_elimina_del_listado()
    {
        var propietario = await ClienteAutenticadoAsync();
        var interesado = await ClienteAutenticadoAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto favorito para quitar test");

        await interesado.PostAsJsonAsync("/api/v1/favorites", new AgregarFavoritoRequest(objetoId));

        var quitar = await interesado.DeleteAsync($"/api/v1/favorites/{objetoId}");
        quitar.StatusCode.Should().Be(HttpStatusCode.OK);

        var listado = await interesado.GetAsync("/api/v1/favorites");
        var contenido = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should().BeEmpty();
    }
}
