using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// ExchangeFlowTests cubre la maquina de estados; esta clase cubre las
// consultas GET /exchanges y GET /exchanges/{id} que ese archivo no ejercita.
public class ExchangesQueryTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public ExchangesQueryTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId)> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, login.Usuario.Id);
    }

    [Fact]
    public async Task ObtenerMisIntercambios_lista_los_intercambios_donde_participo()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto lista intercambios a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto lista intercambios b");
        var crear = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, null));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var intercambioId = contenidoCrear!.Data.GetProperty("id").GetGuid();

        var respuestaSolicitante = await solicitante.GetAsync("/api/v1/exchanges");
        var respuestaPropietario = await propietario.GetAsync("/api/v1/exchanges");

        respuestaSolicitante.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenidoSolicitante = await respuestaSolicitante.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenidoSolicitante!.Data.EnumerateArray().Should().Contain(i => i.GetProperty("id").GetGuid() == intercambioId);

        var contenidoPropietario = await respuestaPropietario.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenidoPropietario!.Data.EnumerateArray().Should().Contain(i => i.GetProperty("id").GetGuid() == intercambioId);
    }

    [Fact]
    public async Task ObtenerPorId_devuelve_el_detalle_para_un_participante()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto detalle intercambio a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto detalle intercambio b");
        var crear = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, "Mensaje de prueba"));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var intercambioId = contenidoCrear!.Data.GetProperty("id").GetGuid();

        var respuesta = await propietario.GetAsync($"/api/v1/exchanges/{intercambioId}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("mensajeInicial").GetString().Should().Be("Mensaje de prueba");
    }

    [Fact]
    public async Task ObtenerPorId_para_un_tercero_ajeno_devuelve_403()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var (ajeno, _) = await ClienteAutenticadoAsync();
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto tercero detalle a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto tercero detalle b");
        var crear = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, null));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var intercambioId = contenidoCrear!.Data.GetProperty("id").GetGuid();

        var respuesta = await ajeno.GetAsync($"/api/v1/exchanges/{intercambioId}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ObtenerPorId_de_un_intercambio_inexistente_devuelve_404()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();

        var respuesta = await propietario.GetAsync($"/api/v1/exchanges/{Guid.NewGuid()}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
