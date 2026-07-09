using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Auth.Commands.CerrarSesion;
using ExchangePlatform.Application.Features.Auth.Commands.OlvidarPassword;
using ExchangePlatform.Application.Features.Auth.Commands.RenovarToken;
using ExchangePlatform.Application.Features.Auth.Commands.RestablecerPassword;
using ExchangePlatform.Application.Features.Auth.DTOs;
using ExchangePlatform.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.IntegrationTests;

// Cubre los flujos de AuthController que ExchangeFlowTests/FavoritesTests/etc.
// no ejercitan: renovar token, cerrar sesion y recuperacion de contrasena
// (via el CapturingEmailService registrado en CustomWebApplicationFactory
// en vez de LogEmailService real).
public class AuthFlowsTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);
    private readonly CustomWebApplicationFactory _factory;

    public AuthFlowsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private static string ExtraerTokenDelCuerpo(string cuerpo)
    {
        var coincidencia = Regex.Match(cuerpo, @"token=(\S+)");
        coincidencia.Success.Should().BeTrue("el cuerpo del correo debe incluir el enlace con el token");
        return Uri.UnescapeDataString(coincidencia.Groups[1].Value);
    }

    [Fact]
    public async Task RenovarToken_rota_el_refresh_token_y_permite_seguir_autenticado()
    {
        var client = _factory.CreateClient();
        var (email, password) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var login = await TestAuthHelper.LoginAsync(client, email, password);

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RenovarTokenCommand(login.RefreshToken));

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>(JsonOpciones);
        contenido!.Data!.AccessToken.Should().NotBeNullOrEmpty();
        contenido.Data.RefreshToken.Should().NotBe(login.RefreshToken);

        // El token original ya fue rotado (revocado): reutilizarlo debe fallar.
        var reutilizar = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RenovarTokenCommand(login.RefreshToken));
        reutilizar.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RenovarToken_con_token_inexistente_devuelve_422()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RenovarTokenCommand("token-que-no-existe"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CerrarSesion_revoca_el_refresh_token()
    {
        var client = _factory.CreateClient();
        var (email, password) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var login = await TestAuthHelper.LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/logout",
            new CerrarSesionCommand(login.RefreshToken));
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        var intentoRenovar = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RenovarTokenCommand(login.RefreshToken));
        intentoRenovar.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CerrarSesion_con_token_ya_invalido_no_falla()
    {
        var client = _factory.CreateClient();
        var (email, password) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var login = await TestAuthHelper.LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/logout",
            new CerrarSesionCommand("token-que-no-existe"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CerrarSesion_sin_autenticacion_devuelve_401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/logout",
            new CerrarSesionCommand("cualquier-token"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OlvidarPassword_para_correo_registrado_envia_un_enlace_de_recuperacion()
    {
        var client = _factory.CreateClient();
        var (email, _) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var emailService = (CapturingEmailService)_factory.Services.GetRequiredService<IEmailService>();

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new OlvidarPasswordCommand(email));

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        emailService.UltimoDestinatario.Should().Be(email);
        emailService.UltimoCuerpo.Should().Contain("reset-password?token=");
    }

    [Fact]
    public async Task OlvidarPassword_para_correo_no_registrado_no_envia_correo_y_responde_200()
    {
        var client = _factory.CreateClient();
        var emailService = (CapturingEmailService)_factory.Services.GetRequiredService<IEmailService>();
        var destinatarioAntes = emailService.UltimoDestinatario;

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new OlvidarPasswordCommand("no-registrado-nunca@example.com"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        emailService.UltimoDestinatario.Should().Be(destinatarioAntes);
    }

    [Fact]
    public async Task OlvidarPassword_con_correo_con_formato_invalido_devuelve_422()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new OlvidarPasswordCommand("no-es-un-correo"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Flujo_completo_de_recuperacion_permite_iniciar_sesion_con_la_nueva_contrasena()
    {
        var client = _factory.CreateClient();
        var (email, passwordOriginal) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var loginOriginal = await TestAuthHelper.LoginAsync(client, email, passwordOriginal);
        var emailService = (CapturingEmailService)_factory.Services.GetRequiredService<IEmailService>();

        await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new OlvidarPasswordCommand(email));
        var token = ExtraerTokenDelCuerpo(emailService.UltimoCuerpo!);

        var restablecer = await client.PostAsJsonAsync("/api/v1/auth/reset-password",
            new RestablecerPasswordCommand(token, "ClaveRecuperada@789", "ClaveRecuperada@789"));
        restablecer.StatusCode.Should().Be(HttpStatusCode.OK);

        var nuevoLogin = await TestAuthHelper.LoginAsync(client, email, "ClaveRecuperada@789");
        nuevoLogin.AccessToken.Should().NotBeNullOrEmpty();

        // Medida de seguridad del handler: restablecer revoca todas las
        // sesiones activas previas del usuario.
        var intentoRenovarSesionVieja = await client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RenovarTokenCommand(loginOriginal.RefreshToken));
        intentoRenovarSesionVieja.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RestablecerPassword_con_token_inexistente_devuelve_409()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/reset-password",
            new RestablecerPasswordCommand("token-inexistente", "ClaveValida@123", "ClaveValida@123"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RestablecerPassword_con_contrasena_debil_devuelve_422()
    {
        var client = _factory.CreateClient();
        var (email, _) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var emailService = (CapturingEmailService)_factory.Services.GetRequiredService<IEmailService>();
        await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new OlvidarPasswordCommand(email));
        var token = ExtraerTokenDelCuerpo(emailService.UltimoCuerpo!);

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/reset-password",
            new RestablecerPasswordCommand(token, "debil", "debil"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RestablecerPassword_con_confirmacion_distinta_devuelve_422()
    {
        var client = _factory.CreateClient();
        var (email, _) = await TestAuthHelper.RegistrarUsuarioAsync(client);
        var emailService = (CapturingEmailService)_factory.Services.GetRequiredService<IEmailService>();
        await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new OlvidarPasswordCommand(email));
        var token = ExtraerTokenDelCuerpo(emailService.UltimoCuerpo!);

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/reset-password",
            new RestablecerPasswordCommand(token, "ClaveValida@123", "OtraClaveDistinta@456"));

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
