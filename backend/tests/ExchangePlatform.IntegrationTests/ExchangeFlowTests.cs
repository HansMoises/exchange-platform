using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using ExchangePlatform.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.IntegrationTests;

// Cubre la maquina de estados de Intercambio (Domain/Entities/Intercambio.cs):
// Pendiente -> Aceptado -> PendienteConfirmacion -> Completado, con las
// reglas de negocio de quien puede hacer cada transicion.
public class ExchangeFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public ExchangeFlowTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId)> ClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, login.Usuario.Id);
    }

    private static async Task<Guid> CrearIntercambioAsync(
        HttpClient solicitante, Guid objetoSolicitado, Guid objetoOfrecido)
    {
        var respuesta = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoOfrecido, "Hola, me interesa tu objeto."));
        respuesta.EnsureSuccessStatusCode();

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        return contenido!.Data.GetProperty("id").GetGuid();
    }

    private async Task<string> ObtenerEstadoObjetoAsync(Guid objetoId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();
        var objeto = await db.Objetos.AsNoTracking().FirstAsync(o => o.Id == objetoId);
        return objeto.Estado.ToString();
    }

    private async Task<int> ObtenerTotalIntercambiosAsync(Guid usuarioId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();
        var usuario = await db.Usuarios.AsNoTracking().FirstAsync(u => u.Id == usuarioId);
        return usuario.TotalIntercambios;
    }

    [Fact]
    public async Task Flujo_completo_termina_en_Completado_y_marca_ambos_objetos_intercambiados()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();

        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto del propietario valido");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto del solicitante valido");

        var intercambioId = await CrearIntercambioAsync(solicitante, objetoSolicitado, objetoOfrecido);

        // Aceptar: reserva ambos objetos.
        var aceptar = await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/accept", null);
        aceptar.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ObtenerEstadoObjetoAsync(objetoSolicitado)).Should().Be("Reservado");
        (await ObtenerEstadoObjetoAsync(objetoOfrecido)).Should().Be("Reservado");

        // Confirma el propietario primero: queda pendiente de la otra confirmacion.
        var primeraConfirmacion = await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/confirm", null);
        primeraConfirmacion.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtoParcial = await primeraConfirmacion.Content
            .ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        dtoParcial!.Data.GetProperty("estado").GetString().Should().Be("PendienteConfirmacion");

        // Confirma el solicitante: se completa.
        var segundaConfirmacion = await solicitante.PatchAsync($"/api/v1/exchanges/{intercambioId}/confirm", null);
        segundaConfirmacion.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtoFinal = await segundaConfirmacion.Content
            .ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        dtoFinal!.Data.GetProperty("estado").GetString().Should().Be("Completado");

        (await ObtenerEstadoObjetoAsync(objetoSolicitado)).Should().Be("Intercambiado");
        (await ObtenerEstadoObjetoAsync(objetoOfrecido)).Should().Be("Intercambiado");
    }

    [Fact]
    public async Task Solo_el_propietario_puede_aceptar_o_rechazar()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();

        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto solicitado valido uno");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto ofrecido valido dos");
        var intercambioId = await CrearIntercambioAsync(solicitante, objetoSolicitado, objetoOfrecido);

        // El propio solicitante no puede aceptar su propia solicitud.
        var aceptarComoSolicitante = await solicitante.PatchAsync($"/api/v1/exchanges/{intercambioId}/accept", null);
        aceptarComoSolicitante.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var rechazar = await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/reject", null);
        rechazar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cancelar_libera_los_objetos_reservados()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();

        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto para cancelar prueba a");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto para cancelar prueba b");
        var intercambioId = await CrearIntercambioAsync(solicitante, objetoSolicitado, objetoOfrecido);

        await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/accept", null);
        (await ObtenerEstadoObjetoAsync(objetoSolicitado)).Should().Be("Reservado");

        var cancelar = await solicitante.PatchAsync($"/api/v1/exchanges/{intercambioId}/cancel", null);
        cancelar.StatusCode.Should().Be(HttpStatusCode.OK);

        (await ObtenerEstadoObjetoAsync(objetoSolicitado)).Should().Be("Disponible");
        (await ObtenerEstadoObjetoAsync(objetoOfrecido)).Should().Be("Disponible");
    }

    [Fact]
    public async Task No_se_puede_solicitar_el_propio_objeto()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var objetoPropio1 = await TestObjectHelper.CrearObjetoAsync(propietario, "Primer objeto propio valido");
        var objetoPropio2 = await TestObjectHelper.CrearObjetoAsync(propietario, "Segundo objeto propio valido");

        var respuesta = await propietario.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoPropio1, objetoPropio2, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task El_objeto_ofrecido_debe_pertenecer_al_solicitante()
    {
        var (propietario, _) = await ClienteAutenticadoAsync();
        var (solicitante, _) = await ClienteAutenticadoAsync();
        var (terceroAjeno, _) = await ClienteAutenticadoAsync();

        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto de otro dueno valido");
        var objetoDeUnTercero = await TestObjectHelper.CrearObjetoAsync(terceroAjeno, "Objeto de un tercero valido");

        var respuesta = await solicitante.PostAsJsonAsync("/api/v1/exchanges",
            new CrearIntercambioRequest(objetoSolicitado, objetoDeUnTercero, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Al_completarse_incrementa_el_contador_de_intercambios_de_ambos()
    {
        var (propietario, idPropietario) = await ClienteAutenticadoAsync();
        var (solicitante, idSolicitante) = await ClienteAutenticadoAsync();

        var objetoSolicitado = await TestObjectHelper.CrearObjetoAsync(propietario, "Objeto contador prueba uno");
        var objetoOfrecido = await TestObjectHelper.CrearObjetoAsync(solicitante, "Objeto contador prueba dos");
        var intercambioId = await CrearIntercambioAsync(solicitante, objetoSolicitado, objetoOfrecido);

        await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/accept", null);
        await propietario.PatchAsync($"/api/v1/exchanges/{intercambioId}/confirm", null);
        await solicitante.PatchAsync($"/api/v1/exchanges/{intercambioId}/confirm", null);

        (await ObtenerTotalIntercambiosAsync(idPropietario)).Should().Be(1);
        (await ObtenerTotalIntercambiosAsync(idSolicitante)).Should().Be(1);
    }
}
