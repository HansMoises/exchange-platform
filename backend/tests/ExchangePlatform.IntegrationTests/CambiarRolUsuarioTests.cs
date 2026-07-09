using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Domain.Common;
using ExchangePlatform.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.IntegrationTests;

// Cobertura funcional (no solo de autorizacion) del endpoint agregado esta
// sesion PATCH /admin/users/{id}/role.
public class CambiarRolUsuarioTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CambiarRolUsuarioTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Administrador_puede_cambiar_el_rol_de_otro_usuario()
    {
        var client = _factory.CreateClient();
        var (tokenAdmin, _) = await TestAuthHelper.CrearUsuarioConRolAsync(
            _factory, client, RolesConocidos.Administrador);
        var (_, loginObjetivo) = await TestAuthHelper.CrearUsuarioConRolAsync(
            _factory, client, RolesConocidos.Usuario);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var respuesta = await client.PatchAsJsonAsync(
            $"/api/v1/admin/users/{loginObjetivo.Usuario.Id}/role",
            new CambiarRolRequest(RolesConocidos.Moderador));

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();
        var usuario = await db.Usuarios.AsNoTracking().FirstAsync(u => u.Id == loginObjetivo.Usuario.Id);
        usuario.RolId.Should().Be(RolesConocidos.Moderador);
    }

    [Fact]
    public async Task Moderador_no_puede_cambiar_roles()
    {
        var client = _factory.CreateClient();
        var (tokenModerador, _) = await TestAuthHelper.CrearUsuarioConRolAsync(
            _factory, client, RolesConocidos.Moderador);
        var (_, loginObjetivo) = await TestAuthHelper.CrearUsuarioConRolAsync(
            _factory, client, RolesConocidos.Usuario);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModerador);

        var respuesta = await client.PatchAsJsonAsync(
            $"/api/v1/admin/users/{loginObjetivo.Usuario.Id}/role",
            new CambiarRolRequest(RolesConocidos.Administrador));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Rechaza_un_rolId_que_no_existe_con_422()
    {
        var client = _factory.CreateClient();
        var (tokenAdmin, _) = await TestAuthHelper.CrearUsuarioConRolAsync(
            _factory, client, RolesConocidos.Administrador);
        var (_, loginObjetivo) = await TestAuthHelper.CrearUsuarioConRolAsync(
            _factory, client, RolesConocidos.Usuario);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var respuesta = await client.PatchAsJsonAsync(
            $"/api/v1/admin/users/{loginObjetivo.Usuario.Id}/role",
            new CambiarRolRequest(999));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
