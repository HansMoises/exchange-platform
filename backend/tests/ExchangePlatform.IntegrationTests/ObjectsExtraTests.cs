using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// ObjectsTests.cs cubre crear/leer/actualizar/eliminar; esta clase cubre
// GET /objects/me, GET /objects/me/available y POST /objects/images, que
// ese archivo no ejercita.
public class ObjectsExtraTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public ObjectsExtraTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task ObtenerMisObjetos_lista_todos_los_objetos_del_usuario_sin_importar_estado()
    {
        var client = await ClienteAutenticadoAsync();
        var id = await TestObjectHelper.CrearObjetoAsync(client, "Objeto mis objetos valido");

        var respuesta = await client.GetAsync("/api/v1/objects/me");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should().Contain(o => o.GetProperty("id").GetGuid() == id);
    }

    [Fact]
    public async Task ObtenerMisObjetos_sin_autenticacion_devuelve_401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/v1/objects/me");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerMisObjetosDisponibles_solo_lista_los_disponibles()
    {
        var propietario = await ClienteAutenticadoAsync();
        var solicitante = await ClienteAutenticadoAsync();
        var objetoDisponible = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto disponible valido");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto que sera reservado valido");
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto solicitado para reservar");

        var crear = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, null));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var intercambioId = contenidoCrear!.Data.GetProperty("id").GetGuid();
        await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/accept", null);

        var respuesta = await solicitante.GetAsync("/api/v1/objects/me/available");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var idsDisponibles = contenido!.Data.EnumerateArray().Select(o => o.GetProperty("id").GetGuid()).ToList();
        idsDisponibles.Should().NotContain(objetoOfrecido, "quedo reservado al aceptarse el intercambio");
    }

    [Fact]
    public async Task SubirImagen_valida_devuelve_la_url()
    {
        var client = await ClienteAutenticadoAsync();

        using var contenidoMultipart = new MultipartFormDataContent();
        var archivo = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });
        archivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        contenidoMultipart.Add(archivo, "archivo", "foto.jpg");

        var respuesta = await client.PostAsync("/api/v1/objects/images", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("url").GetString().Should().Contain("/uploads/");
    }

    [Fact]
    public async Task SubirImagen_sin_archivo_devuelve_400()
    {
        var client = await ClienteAutenticadoAsync();

        using var contenidoMultipart = new MultipartFormDataContent();

        var respuesta = await client.PostAsync("/api/v1/objects/images", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubirImagen_con_tipo_no_permitido_devuelve_422()
    {
        var client = await ClienteAutenticadoAsync();

        using var contenidoMultipart = new MultipartFormDataContent();
        var archivo = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        archivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        contenidoMultipart.Add(archivo, "archivo", "documento.pdf");

        var respuesta = await client.PostAsync("/api/v1/objects/images", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SubirImagen_sin_autenticacion_devuelve_401()
    {
        var client = _factory.CreateClient();

        using var contenidoMultipart = new MultipartFormDataContent();
        var archivo = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        archivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        contenidoMultipart.Add(archivo, "archivo", "foto.jpg");

        var respuesta = await client.PostAsync("/api/v1/objects/images", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
