using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// Notificacion se crea como efecto secundario de proponer un intercambio
// (CrearIntercambioCommandHandler notifica al propietario del objeto
// solicitado), asi que reutilizamos ese flujo real en vez de insertar
// notificaciones a mano.
public class NotificationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public NotificationsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId)> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, login.Usuario.Id);
    }

    private static async Task<Guid> GenerarNotificacionParaPropietarioAsync(
        HttpClient propietario, HttpClient solicitante)
    {
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto que genera notificacion a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto que genera notificacion b");

        var respuesta = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, "Hola, me interesa."));
        respuesta.EnsureSuccessStatusCode();
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        return contenido!.Data.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task ObtenerNotificaciones_lista_las_del_usuario_autenticado()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var intercambioId = await GenerarNotificacionParaPropietarioAsync(propietario, solicitante);

        var respuesta = await propietario.GetAsync("/api/v1/notifications");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should().Contain(n =>
            n.GetProperty("tipo").GetString() == "SolicitudRecibida"
            && n.GetProperty("entidadId").GetGuid() == intercambioId
            && n.GetProperty("isLeida").GetBoolean() == false);
    }

    [Fact]
    public async Task MarcarLeida_marca_la_notificacion_propia_como_leida()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        await GenerarNotificacionParaPropietarioAsync(propietario, solicitante);

        var listado = await propietario.GetAsync("/api/v1/notifications");
        var contenidoListado = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var notificacionId = contenidoListado!.Data.EnumerateArray().First().GetProperty("id").GetGuid();

        var marcar = await propietario.PatchAsync($"/api/v1/notifications/{notificacionId}/read", null);
        marcar.StatusCode.Should().Be(HttpStatusCode.OK);

        var listadoActualizado = await propietario.GetAsync("/api/v1/notifications");
        var contenidoActualizado = await listadoActualizado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenidoActualizado!.Data.EnumerateArray()
            .Should().Contain(n => n.GetProperty("id").GetGuid() == notificacionId
                && n.GetProperty("isLeida").GetBoolean() == true);
    }

    [Fact]
    public async Task MarcarLeida_de_una_notificacion_ajena_devuelve_403()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var (ajeno, _) = await ClienteAutenticadoAsync();
        await GenerarNotificacionParaPropietarioAsync(propietario, solicitante);

        var listado = await propietario.GetAsync("/api/v1/notifications");
        var contenidoListado = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var notificacionId = contenidoListado!.Data.EnumerateArray().First().GetProperty("id").GetGuid();

        var respuesta = await ajeno.PatchAsync($"/api/v1/notifications/{notificacionId}/read", null);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MarcarLeida_de_una_notificacion_inexistente_devuelve_404()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();

        var respuesta = await propietario.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/read", null);

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarcarTodasLeidas_deja_sin_pendientes_al_usuario()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitanteUno, _) = await ClienteAutenticadoAsync();
        var (solicitanteDos, _) = await ClienteAutenticadoAsync();
        await GenerarNotificacionParaPropietarioAsync(propietario, solicitanteUno);
        await GenerarNotificacionParaPropietarioAsync(propietario, solicitanteDos);

        var marcarTodas = await propietario.PatchAsync("/api/v1/notifications/read-all", null);
        marcarTodas.StatusCode.Should().Be(HttpStatusCode.OK);

        var listado = await propietario.GetAsync("/api/v1/notifications");
        var contenido = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should().OnlyContain(n => n.GetProperty("isLeida").GetBoolean() == true);
    }

    [Fact]
    public async Task Sin_autenticacion_devuelve_401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/v1/notifications");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
