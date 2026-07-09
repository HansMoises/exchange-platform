using AppValidationException = ExchangePlatform.Application.Common.Exceptions.ValidationException;
using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Application.Features.Auth.Commands.IniciarSesion;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Repositories;
using ExchangePlatform.Domain.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace ExchangePlatform.UnitTests.Application.Auth;

public class IniciarSesionCommandHandlerTests
{
    private static Usuario CrearUsuario() =>
        new("Rosa", "Quispe", "rosa@example.com", "hash-correcto", "987654321", 1, 1, 1);

    private sealed record Contexto(
        IniciarSesionCommandHandler Handler,
        Mock<IUnitOfWork> Uow,
        Mock<IUsuarioRepository> Usuarios,
        Mock<IPasswordHasher> Hasher,
        Mock<IJwtService> Jwt);

    private static Contexto CrearContexto()
    {
        var uow = new Mock<IUnitOfWork>();
        var usuarios = new Mock<IUsuarioRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtService>();
        var refreshTokens = new Mock<IRefreshTokenRepository>();

        refreshTokens.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        uow.SetupGet(u => u.Usuarios).Returns(usuarios.Object);
        uow.SetupGet(u => u.RefreshTokens).Returns(refreshTokens.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new Contexto(
            new IniciarSesionCommandHandler(uow.Object, hasher.Object, jwt.Object),
            uow, usuarios, hasher, jwt);
    }

    [Fact]
    public async Task Lanza_si_el_usuario_no_existe()
    {
        var c = CrearContexto();
        c.Usuarios.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

        Func<Task> act = () => c.Handler.Handle(new IniciarSesionCommand("x@x.com", "clave"), CancellationToken.None);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task Lanza_ConflictException_si_la_cuenta_esta_bloqueada()
    {
        var c = CrearContexto();
        var usuario = CrearUsuario();
        for (var i = 0; i < 5; i++) usuario.RegistrarIntentoFallido();
        c.Usuarios.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(usuario);

        Func<Task> act = () => c.Handler.Handle(
            new IniciarSesionCommand("rosa@example.com", "clave"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Password_incorrecto_registra_intento_fallido_y_lanza()
    {
        var c = CrearContexto();
        var usuario = CrearUsuario();
        c.Usuarios.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(usuario);
        c.Hasher.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        Func<Task> act = () => c.Handler.Handle(
            new IniciarSesionCommand("rosa@example.com", "mala"), CancellationToken.None);

        await act.Should().ThrowAsync<AppValidationException>();
        usuario.FailedAttempts.Should().Be(1);
        c.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_exitoso_devuelve_tokens_y_datos_del_usuario()
    {
        var c = CrearContexto();
        var usuario = CrearUsuario();
        c.Usuarios.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(usuario);
        c.Hasher.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        c.Jwt.Setup(j => j.GenerarAccessToken(It.IsAny<Usuario>())).Returns("access-token");
        c.Jwt.Setup(j => j.GenerarRefreshToken()).Returns("refresh-token");

        var result = await c.Handler.Handle(
            new IniciarSesionCommand("rosa@example.com", "buena"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Usuario.Email.Should().Be("rosa@example.com");
    }
}
