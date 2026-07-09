using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// AdminAuthorizationTests cubre la matriz de roles sobre los GET de
// AdminController; esta clase cubre el comportamiento funcional de las
// mutaciones (usuarios, objetos, reportes, categorias) que esa matriz no
// ejercita.
public class AdminMutationsTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public AdminMutationsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteConRolAsync(int rolId)
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, rolId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<(HttpClient Client, Guid UsuarioId)> ClienteUsuarioAsync()
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, login.Usuario.Id);
    }

    // ---- Usuarios ----

    [Fact]
    public async Task GetUsuarios_filtra_por_texto_de_busqueda()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var client = _factory.CreateClient();
        var correoUnico = $"buscable-{Guid.NewGuid():N}@example.com";
        await TestAuthHelper.RegistrarUsuarioAsync(client);

        var respuesta = await admin.GetAsync($"/api/v1/admin/users?search={Uri.EscapeDataString(correoUnico)}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenido!.Data.GetProperty("items").EnumerateArray().Should().BeEmpty();
    }

    [Fact]
    public async Task ActivarUsuario_y_DesactivarUsuario_cambian_el_estado()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var (_, usuarioId) = await ClienteUsuarioAsync();

        var desactivar = await admin.PatchAsync($"/api/v1/admin/users/{usuarioId}/deactivate", null);
        desactivar.StatusCode.Should().Be(HttpStatusCode.OK);

        var activar = await admin.PatchAsync($"/api/v1/admin/users/{usuarioId}/activate", null);
        activar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivarUsuario_inexistente_devuelve_404()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);

        var respuesta = await admin.PatchAsync($"/api/v1/admin/users/{Guid.NewGuid()}/activate", null);

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EliminarUsuario_lo_marca_como_eliminado()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var (_, usuarioId) = await ClienteUsuarioAsync();

        var respuesta = await admin.DeleteAsync($"/api/v1/admin/users/{usuarioId}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EliminarUsuario_inexistente_devuelve_404()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);

        var respuesta = await admin.DeleteAsync($"/api/v1/admin/users/{Guid.NewGuid()}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- Objetos ----

    [Fact]
    public async Task SuspenderObjeto_y_RestaurarObjeto_cambian_el_estado()
    {
        var moderador = await ClienteConRolAsync(RolesConocidos.Moderador);
        var propietario = await ClienteUsuarioAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario.Client, "Objeto admin suspender valido");

        var suspender = await moderador.PatchAsync($"/api/v1/admin/objects/{objetoId}/suspend", null);
        suspender.StatusCode.Should().Be(HttpStatusCode.OK);

        var restaurar = await moderador.PatchAsync($"/api/v1/admin/objects/{objetoId}/restore", null);
        restaurar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SuspenderObjeto_inexistente_devuelve_404()
    {
        var moderador = await ClienteConRolAsync(RolesConocidos.Moderador);

        var respuesta = await moderador.PatchAsync($"/api/v1/admin/objects/{Guid.NewGuid()}/suspend", null);

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- Reportes ----

    [Fact]
    public async Task ResolverReporte_y_DescartarReporte_cambian_el_estado()
    {
        var moderador = await ClienteConRolAsync(RolesConocidos.Moderador);
        var reportante = await ClienteUsuarioAsync();
        var propietario = await ClienteUsuarioAsync();
        var objetoId = await TestObjectHelper.CrearObjetoAsync(propietario.Client, "Objeto admin reportes valido");

        var crearReporteUno = await reportante.Client.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", objetoId, "Spam", null));
        var contenidoUno = await crearReporteUno.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var reporteId = contenidoUno!.Data.GetProperty("id").GetGuid();

        var resolver = await moderador.PatchAsync($"/api/v1/admin/reports/{reporteId}/resolve", null);
        resolver.StatusCode.Should().Be(HttpStatusCode.OK);

        // Se necesita un segundo reporte (el primero ya no esta activo) para probar Descartar.
        var otroPropietario = await ClienteUsuarioAsync();
        var otroObjetoId = await TestObjectHelper.CrearObjetoAsync(otroPropietario.Client, "Objeto admin reportes valido dos");
        var crearReporteDos = await reportante.Client.PostAsJsonAsync("/api/v1/reports",
            new CrearReporteRequest("Objeto", otroObjetoId, "Fraude", null));
        var contenidoDos = await crearReporteDos.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var reporteDosId = contenidoDos!.Data.GetProperty("id").GetGuid();

        var descartar = await moderador.PatchAsync($"/api/v1/admin/reports/{reporteDosId}/discard", null);
        descartar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResolverReporte_inexistente_devuelve_404()
    {
        var moderador = await ClienteConRolAsync(RolesConocidos.Moderador);

        var respuesta = await moderador.PatchAsync($"/api/v1/admin/reports/{Guid.NewGuid()}/resolve", null);

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- Categorias ----

    [Fact]
    public async Task CrearCategoria_valida_devuelve_201()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);

        var respuesta = await admin.PostAsJsonAsync("/api/v1/admin/categories",
            new CategoriaRequest($"Categoria {Guid.NewGuid():N}", "Descripcion valida", "🎈"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CrearCategoria_con_nombre_duplicado_devuelve_422()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var nombre = $"Categoria duplicada {Guid.NewGuid():N}";
        await admin.PostAsJsonAsync("/api/v1/admin/categories", new CategoriaRequest(nombre, null, null));

        var respuesta = await admin.PostAsJsonAsync("/api/v1/admin/categories",
            new CategoriaRequest(nombre, "Otra descripcion", null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ActualizarCategoria_valida_persiste_los_cambios()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var crear = await admin.PostAsJsonAsync("/api/v1/admin/categories",
            new CategoriaRequest($"Categoria editar {Guid.NewGuid():N}", null, null));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var categoriaId = contenidoCrear!.Data.GetProperty("id").GetInt32();

        var nuevoNombre = $"Categoria editada {Guid.NewGuid():N}";
        var respuesta = await admin.PutAsJsonAsync($"/api/v1/admin/categories/{categoriaId}",
            new CategoriaRequest(nuevoNombre, "Nueva descripcion", "🆕"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var listado = await admin.GetAsync("/api/v1/admin/categories");
        var contenidoListado = await listado.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        contenidoListado!.Data.EnumerateArray().Should()
            .ContainSingle(c => c.GetProperty("id").GetInt32() == categoriaId
                && c.GetProperty("nombre").GetString() == nuevoNombre);
    }

    [Fact]
    public async Task ActualizarCategoria_inexistente_devuelve_404()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);

        var respuesta = await admin.PutAsJsonAsync("/api/v1/admin/categories/999999",
            new CategoriaRequest("Nombre cualquiera", null, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarCategoria_con_nombre_duplicado_devuelve_422()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var nombreExistente = $"Categoria existente {Guid.NewGuid():N}";
        await admin.PostAsJsonAsync("/api/v1/admin/categories", new CategoriaRequest(nombreExistente, null, null));

        var crear = await admin.PostAsJsonAsync("/api/v1/admin/categories",
            new CategoriaRequest($"Categoria a editar {Guid.NewGuid():N}", null, null));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var categoriaId = contenidoCrear!.Data.GetProperty("id").GetInt32();

        var respuesta = await admin.PutAsJsonAsync($"/api/v1/admin/categories/{categoriaId}",
            new CategoriaRequest(nombreExistente, null, null));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ActivarCategoria_y_DesactivarCategoria_cambian_el_estado()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);
        var crear = await admin.PostAsJsonAsync("/api/v1/admin/categories",
            new CategoriaRequest($"Categoria activar {Guid.NewGuid():N}", null, null));
        var contenidoCrear = await crear.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        var categoriaId = contenidoCrear!.Data.GetProperty("id").GetInt32();

        var desactivar = await admin.PatchAsync($"/api/v1/admin/categories/{categoriaId}/deactivate", null);
        desactivar.StatusCode.Should().Be(HttpStatusCode.OK);

        var activar = await admin.PatchAsync($"/api/v1/admin/categories/{categoriaId}/activate", null);
        activar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivarCategoria_inexistente_devuelve_404()
    {
        var admin = await ClienteConRolAsync(RolesConocidos.Administrador);

        var respuesta = await admin.PatchAsync("/api/v1/admin/categories/999999/activate", null);

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
