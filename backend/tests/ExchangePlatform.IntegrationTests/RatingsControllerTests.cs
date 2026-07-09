using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

public class RatingsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public RatingsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId)> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, login.Usuario.Id);
    }

    // Reproduce el flujo completo Pendiente -> Aceptado -> PendienteConfirmacion
    // -> Completado (ver ExchangeFlowTests) porque solo se puede calificar un
    // intercambio ya completado (RN de CrearCalificacionCommandHandler).
    private static async Task<Guid> CompletarIntercambioAsync(
        HttpClient propietario, HttpClient solicitante, Guid objetoSolicitado, Guid objetoOfrecido)
    {
        var crear = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, null));
        crear.EnsureSuccessStatusCode();
        var contenido = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var intercambioId = contenido!.Data.GetProperty("id").GetGuid();

        (await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/accept", null)).EnsureSuccessStatusCode();
        (await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/confirm", null)).EnsureSuccessStatusCode();
        (await solicitante.PatchAsync($"/api/v1/exchanges/{intercambioId}/confirm", null)).EnsureSuccessStatusCode();

        return intercambioId;
    }

    [Fact]
    public async Task Calificar_un_intercambio_completado_devuelve_201()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, idSolicitante) = await ClienteAutenticadoAsync();
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto para calificar valido a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto para calificar valido b");
        var intercambioId = await CompletarIntercambioAsync(propietario, solicitante, objetoSolicitado, objetoOfrecido);

        var respuesta = await propietario.PostAsJsonAsync("/api/v1/ratings",
            new CrearCalificacionRequest(intercambioId, 5, "Excelente intercambio."));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);

        var listado = await propietario.GetAsync($"/api/v1/ratings/user/{idSolicitante}");
        var contenido = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should()
            .ContainSingle(c => c.GetProperty("intercambioId").GetGuid() == intercambioId
                && c.GetProperty("puntuacion").GetInt32() == 5);
    }

    [Fact]
    public async Task No_se_puede_calificar_un_intercambio_pendiente()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto intercambio pendiente a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto intercambio pendiente b");
        var crear = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, null));
        var contenido = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var intercambioId = contenido!.Data.GetProperty("id").GetGuid();

        var respuesta = await propietario.PostAsJsonAsync("/api/v1/ratings",
            new CrearCalificacionRequest(intercambioId, 5, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Un_tercero_ajeno_no_puede_calificar_el_intercambio()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var (ajeno, _) = await ClienteAutenticadoAsync();
        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto tercero ajeno valido a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto tercero ajeno valido b");
        var intercambioId = await CompletarIntercambioAsync(propietario, solicitante, objetoSolicitado, objetoOfrecido);

        var respuesta = await ajeno.PostAsJsonAsync("/api/v1/ratings",
            new CrearCalificacionRequest(intercambioId, 5, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Calificar_un_intercambio_inexistente_devuelve_404()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();

        var respuesta = await propietario.PostAsJsonAsync("/api/v1/ratings",
            new CrearCalificacionRequest(Guid.NewGuid(), 5, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Puntuacion_fuera_de_rango_devuelve_422()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();

        var respuesta = await propietario.PostAsJsonAsync("/api/v1/ratings",
            new CrearCalificacionRequest(Guid.NewGuid(), 6, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ObtenerPorUsuario_sin_calificaciones_devuelve_lista_vacia()
    {
        var (propietario, idPropietario) = await ClienteAutenticadoAsync();

        var respuesta = await propietario.GetAsync($"/api/v1/ratings/user/{idPropietario}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.EnumerateArray().Should().BeEmpty();
    }
}
