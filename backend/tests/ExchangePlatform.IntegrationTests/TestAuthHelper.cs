using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Auth.Commands.IniciarSesion;
using ExchangePlatform.Application.Features.Auth.Commands.RegistrarUsuario;
using ExchangePlatform.Application.Features.Auth.DTOs;
using ExchangePlatform.Domain.Common;
using ExchangePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangePlatform.IntegrationTests;

// Helper compartido: registra usuarios reales via /auth/register, y cuando se
// necesita un rol distinto al default (Usuario), lo asigna directo en la BD
// (no existe ni deberia existir una via publica para auto-promoverse).
public static class TestAuthHelper
{
    // ASP.NET Core serializa las respuestas en camelCase; sin esta opcion
    // System.Text.Json (case-sensitive por defecto) dejaria las propiedades
    // del DTO en sus valores por defecto en silencio, no en error.
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);

    public static async Task<(string Email, string Password)> RegistrarUsuarioAsync(HttpClient client)
    {
        var correo = $"test-{Guid.NewGuid():N}@example.com";
        const string password = "Clave123!";

        var command = new RegistrarUsuarioCommand(
            Nombres: "Test",
            Apellidos: "Usuario",
            Email: correo,
            Password: password,
            ConfirmPassword: password,
            Telefono: "987654321",
            DepartamentoId: 1,
            ProvinciaId: 1,
            DistritoId: 1);

        var respuesta = await client.PostAsJsonAsync("/api/v1/auth/register", command);
        respuesta.EnsureSuccessStatusCode();

        return (correo, password);
    }

    public static async Task AsignarRolPorEmailAsync(CustomWebApplicationFactory factory, string email, int rolId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExchangePlatformDbContext>();
        var usuario = await db.Usuarios.FirstAsync(u => u.Email == email);
        usuario.CambiarRol(rolId);
        await db.SaveChangesAsync();
    }

    public static async Task<LoginResponseDto> LoginAsync(HttpClient client, string email, string password)
    {
        var respuesta = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new IniciarSesionCommand(email, password));
        respuesta.EnsureSuccessStatusCode();

        var contenido = await respuesta.Content
            .ReadFromJsonAsync<ApiResponse<LoginResponseDto>>(JsonOpciones);

        return contenido!.Data!;
    }

    // Registra un usuario nuevo, opcionalmente le asigna un rol distinto al
    // default, e inicia sesion para devolver un token real con el claim de
    // rol correcto (probando el flujo completo, no un token fabricado a mano).
    public static async Task<(string Token, LoginResponseDto Login)> CrearUsuarioConRolAsync(
        CustomWebApplicationFactory factory, HttpClient client, int rolId)
    {
        var (email, password) = await RegistrarUsuarioAsync(client);

        if (rolId != RolesConocidos.Usuario)
        {
            await AsignarRolPorEmailAsync(factory, email, rolId);
        }

        var login = await LoginAsync(client, email, password);
        return (login.AccessToken, login);
    }
}
