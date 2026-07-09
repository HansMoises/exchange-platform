using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId)> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, login.Usuario.Id);
    }

    [Fact]
    public async Task ObtenerPerfil_publico_devuelve_el_perfil_del_id_indicado()
    {
        var (_, usuarioId) = await ClienteAutenticadoAsync();
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync($"/api/v1/users/{usuarioId}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("id").GetGuid().Should().Be(usuarioId);
    }

    [Fact]
    public async Task ObtenerPerfil_de_un_usuario_inexistente_devuelve_404()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerMiPerfil_devuelve_el_perfil_del_usuario_autenticado()
    {
        var (client, usuarioId) = await ClienteAutenticadoAsync();

        var respuesta = await client.GetAsync("/api/v1/users/me");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("id").GetGuid().Should().Be(usuarioId);
    }

    [Fact]
    public async Task ObtenerMiPerfil_sin_autenticacion_devuelve_401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/v1/users/me");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActualizarPerfil_valido_persiste_los_cambios()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        var actualizar = await client.PutAsJsonAsync("/api/v1/users/me",
            new ActualizarPerfilRequest("Rosa Actualizada", "Quispe Actualizada", "987654321", 1, 1, 1));
        actualizar.StatusCode.Should().Be(HttpStatusCode.OK);

        var perfil = await client.GetAsync("/api/v1/users/me");
        var contenido = await perfil.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("nombres").GetString().Should().Be("Rosa Actualizada");
    }

    [Fact]
    public async Task ActualizarPerfil_con_nombres_vacios_devuelve_422()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        var respuesta = await client.PutAsJsonAsync("/api/v1/users/me",
            new ActualizarPerfilRequest("", "Quispe", "987654321", 1, 1, 1));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ActualizarFoto_con_imagen_valida_devuelve_la_url()
    {
        var (client, usuarioId) = await ClienteAutenticadoAsync();

        using var contenidoMultipart = new MultipartFormDataContent();
        var archivo = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });
        archivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        contenidoMultipart.Add(archivo, "archivo", "foto.jpg");

        var respuesta = await client.PatchAsync("/api/v1/users/me/photo", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("url").GetString().Should().Contain("/uploads/");

        var perfil = await client.GetAsync("/api/v1/users/me");
        var contenidoPerfil = await perfil.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenidoPerfil!.Data.GetProperty("fotoPerfil").GetString().Should().Contain("/uploads/");
    }

    [Fact]
    public async Task ActualizarFoto_con_tipo_no_permitido_devuelve_422()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        using var contenidoMultipart = new MultipartFormDataContent();
        var archivo = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        archivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        contenidoMultipart.Add(archivo, "archivo", "documento.pdf");

        var respuesta = await client.PatchAsync("/api/v1/users/me/photo", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ActualizarFoto_sin_archivo_devuelve_400()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        using var contenidoMultipart = new MultipartFormDataContent();

        var respuesta = await client.PatchAsync("/api/v1/users/me/photo", contenidoMultipart);

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CambiarPassword_con_password_actual_correcta_permite_iniciar_sesion_con_la_nueva()
    {
        var client = _factory.CreateClient();
        var (email, passwordActual) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var login = await TestAuthHelper.LoginAsync(client, email, passwordActual);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var respuesta = await client.PatchAsJsonAsync("/api/v1/users/me/password",
            new CambiarPasswordRequest(passwordActual, "ClaveNueva@456", "ClaveNueva@456"));
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var nuevoLogin = await TestAuthHelper.LoginAsync(client, email, "ClaveNueva@456");
        nuevoLogin.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CambiarPassword_con_password_actual_incorrecta_devuelve_422()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        var respuesta = await client.PatchAsJsonAsync("/api/v1/users/me/password",
            new CambiarPasswordRequest("ClaveIncorrecta@1", "ClaveNueva@456", "ClaveNueva@456"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CambiarPassword_con_confirmacion_distinta_devuelve_422()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        var respuesta = await client.PatchAsJsonAsync("/api/v1/users/me/password",
            new CambiarPasswordRequest("Clave123!", "ClaveNueva@456", "OtraClaveDistinta@789"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
