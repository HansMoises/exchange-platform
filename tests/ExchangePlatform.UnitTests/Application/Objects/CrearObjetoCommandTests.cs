using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExchangePlatform.Application.Features.Objects.Commands.CrearObjeto;
using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ExchangePlatform.UnitTests.Application.Objects;

public class CrearObjetoCommandTests
{
    [Fact]
    public void Validator_Rechaza_Titulo_Corto()
    {
        var cmd = new CrearObjetoCommand("abc", "desc corta", 1, Guid.NewGuid(), "Nuevo", 1, 1, 1, new List<string> { "u" });
        var validator = new CrearObjetoCommandValidator();

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_Throws_NotFound_When_Usuario_Missing()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var usuariosRepo = new Mock<Domain.Interfaces.Repositories.IUsuarioRepository>();
        usuariosRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);
        uowMock.SetupGet(u => u.Usuarios).Returns(usuariosRepo.Object);

        var handler = new CrearObjetoCommandHandler(uowMock.Object);

        var cmd = new CrearObjetoCommand("Titulo valido", new string('x', 30), 1, Guid.NewGuid(), "Nuevo", 1, 1, 1, new List<string>{"url"});

        await handler.Invoking(h => h.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handler_Crea_Objeto_Y_Agrega_Imagenes()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var usuariosRepo = new Mock<Domain.Interfaces.Repositories.IUsuarioRepository>();
        usuariosRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Usuario("n","a","e","p", "123456789", 1,1,1));

        var objetosRepo = new Mock<Domain.Interfaces.Repositories.IObjetoRepository>();
        Objeto? captured = null;
        objetosRepo.Setup(r => r.AddAsync(It.IsAny<Objeto>())).Returns<Objeto>(o => { captured = o; return Task.CompletedTask; });

        uowMock.SetupGet(u => u.Usuarios).Returns(usuariosRepo.Object);
        uowMock.SetupGet(u => u.Objetos).Returns(objetosRepo.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CrearObjetoCommandHandler(uowMock.Object);

        var usuarioId = Guid.NewGuid();
        var cmd = new CrearObjetoCommand("Titulo valido", new string('x', 30), 1, usuarioId, "Nuevo", 1, 1, 1, new List<string>{"u1","u2"});

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        captured.Should().NotBeNull();
        captured!.Imagenes.Should().HaveCount(2);
        objetosRepo.Verify(r => r.AddAsync(It.IsAny<Objeto>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
