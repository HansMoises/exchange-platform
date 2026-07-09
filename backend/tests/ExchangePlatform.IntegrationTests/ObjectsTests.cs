using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

public class ObjectsTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public ObjectsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(int rolId = RolesConocidos.Usuario)
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, rolId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Usuario_autenticado_puede_crear_y_leer_su_objeto_con_categoria_poblada()
    {
        var client = await ClienteAutenticadoAsync();
        var id = await TestObjectHelper.CrearObjetoAsync(client, "Bicicleta rodado 26 en buen estado");

        var respuesta = await client.GetAsync($"/api/v1/objects/{id}");
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var objeto = contenido!.Data;

        objeto.GetProperty("titulo").GetString().Should().Be("Bicicleta rodado 26 en buen estado");
        objeto.GetProperty("categoriaNombre").GetString().Should().NotBeNullOrWhiteSpace();
        objeto.GetProperty("estado").GetString().Should().Be("Disponible");
    }

    [Fact]
    public async Task Anonimo_puede_ver_el_listado_y_el_detalle_de_un_objeto()
    {
        var clientePropietario = await ClienteAutenticadoAsync();
        var id = await TestObjectHelper.CrearObjetoAsync(clientePropietario);

        var clienteAnonimo = _factory.CreateClient();

        var listado = await clienteAnonimo.GetAsync("/api/v1/objects");
        listado.StatusCode.Should().Be(HttpStatusCode.OK);

        var detalle = await clienteAnonimo.GetAsync($"/api/v1/objects/{id}");
        detalle.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Solo_el_propietario_puede_actualizar_su_objeto()
    {
        var propietario = await ClienteAutenticadoAsync();
        var id = await TestObjectHelper.CrearObjetoAsync(propietario);

        var otroUsuario = await ClienteAutenticadoAsync();
        var request = new ActualizarObjetoRequest(
            "Titulo actualizado valido",
            "Descripcion actualizada con al menos veinte caracteres.",
            1, "Bueno", 1, 1, 1);

        var respuestaAjena = await otroUsuario.PutAsJsonAsync($"/api/v1/objects/{id}", request);
        respuestaAjena.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var respuestaPropia = await propietario.PutAsJsonAsync($"/api/v1/objects/{id}", request);
        respuestaPropia.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Solo_el_propietario_puede_eliminar_su_objeto()
    {
        var propietario = await ClienteAutenticadoAsync();
        var id = await TestObjectHelper.CrearObjetoAsync(propietario);

        var otroUsuario = await ClienteAutenticadoAsync();
        var respuestaAjena = await otroUsuario.DeleteAsync($"/api/v1/objects/{id}");
        respuestaAjena.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var respuestaPropia = await propietario.DeleteAsync($"/api/v1/objects/{id}");
        respuestaPropia.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
