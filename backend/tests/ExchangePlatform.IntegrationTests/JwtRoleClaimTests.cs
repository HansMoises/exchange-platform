using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// Cubre el bug critico corregido esta sesion: el claim de rol del JWT y el
// campo Usuario.Rol de la respuesta de login devolvian RolId.ToString()
// ("1"/"2"/"3") en vez del nombre del rol, rompiendo en silencio todo
// [Authorize(Roles = "...")] y el RoleBasedRoute del frontend.
public class JwtRoleClaimTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public JwtRoleClaimTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData(RolesConocidos.Usuario, "Usuario")]
    [InlineData(RolesConocidos.Moderador, "Moderador")]
    [InlineData(RolesConocidos.Administrador, "Administrador")]
    public async Task Login_incluye_el_nombre_del_rol_no_el_id_numerico(int rolId, string nombreEsperado)
    {
        var client = _factory.CreateClient();
        var (token, login) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, rolId);

        login.Usuario.Rol.Should().Be(nombreEsperado);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claimRol = jwt.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Role).Which;
        claimRol.Value.Should().Be(nombreEsperado);
    }
}
