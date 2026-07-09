using System.Net;
using System.Net.Http.Headers;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// Cubre la matriz de autorizacion agregada esta sesion a AdminController:
// antes cualquier usuario autenticado (sin importar el rol) podia llamar
// cualquier endpoint /admin/*. Ahora la clase exige Administrador o
// Moderador, y ciertas acciones (usuarios, auditoria, categorias) exigen
// Administrador especificamente (combinando [Authorize] de clase + metodo).
public class AdminAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminAuthorizationTests(CustomWebApplicationFactory factory) => _factory = factory;

    public static IEnumerable<object?[]> CasosMatriz()
    {
        // rolId (null = sin autenticar), ruta, statusEsperado
        yield return new object?[] { null, "/api/v1/admin/dashboard", HttpStatusCode.Unauthorized };
        yield return new object?[] { RolesConocidos.Usuario, "/api/v1/admin/dashboard", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Moderador, "/api/v1/admin/dashboard", HttpStatusCode.OK };
        yield return new object?[] { RolesConocidos.Administrador, "/api/v1/admin/dashboard", HttpStatusCode.OK };

        yield return new object?[] { RolesConocidos.Usuario, "/api/v1/admin/objects", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Moderador, "/api/v1/admin/objects", HttpStatusCode.OK };
        yield return new object?[] { RolesConocidos.Administrador, "/api/v1/admin/objects", HttpStatusCode.OK };

        yield return new object?[] { RolesConocidos.Usuario, "/api/v1/admin/reports", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Moderador, "/api/v1/admin/reports", HttpStatusCode.OK };
        yield return new object?[] { RolesConocidos.Administrador, "/api/v1/admin/reports", HttpStatusCode.OK };

        // Estas 3 exigen Administrador especificamente: Moderador debe recibir 403.
        yield return new object?[] { RolesConocidos.Usuario, "/api/v1/admin/users", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Moderador, "/api/v1/admin/users", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Administrador, "/api/v1/admin/users", HttpStatusCode.OK };

        yield return new object?[] { RolesConocidos.Usuario, "/api/v1/admin/audit-logs", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Moderador, "/api/v1/admin/audit-logs", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Administrador, "/api/v1/admin/audit-logs", HttpStatusCode.OK };

        yield return new object?[] { RolesConocidos.Usuario, "/api/v1/admin/categories", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Moderador, "/api/v1/admin/categories", HttpStatusCode.Forbidden };
        yield return new object?[] { RolesConocidos.Administrador, "/api/v1/admin/categories", HttpStatusCode.OK };
    }

    [Theory]
    [MemberData(nameof(CasosMatriz))]
    public async Task Ruta_admin_respeta_la_matriz_de_roles(int? rolId, string ruta, HttpStatusCode esperado)
    {
        var client = _factory.CreateClient();

        if (rolId.HasValue)
        {
            var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, rolId.Value);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var respuesta = await client.GetAsync(ruta);

        respuesta.StatusCode.Should().Be(esperado);
    }
}
