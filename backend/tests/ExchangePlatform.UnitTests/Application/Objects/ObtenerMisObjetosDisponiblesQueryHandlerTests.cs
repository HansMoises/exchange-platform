using ExchangePlatform.Application.Features.Objects.Queries.ObtenerMisObjetosDisponibles;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Entities.Maestras;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ExchangePlatform.UnitTests.Application.Objects;

public class ObtenerMisObjetosDisponiblesQueryHandlerTests
{
    private static (ObtenerMisObjetosDisponiblesQueryHandler Handler,
        Mock<IObjetoRepository> Objetos, Mock<IUsuarioRepository> Usuarios, Mock<ICategoriaRepository> Categorias)
        CrearContexto()
    {
        var uow = new Mock<IUnitOfWork>();
        var objetos = new Mock<IObjetoRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        var categorias = new Mock<ICategoriaRepository>();

        uow.SetupGet(u => u.Objetos).Returns(objetos.Object);
        uow.SetupGet(u => u.Usuarios).Returns(usuarios.Object);
        uow.SetupGet(u => u.Categorias).Returns(categorias.Object);

        return (new ObtenerMisObjetosDisponiblesQueryHandler(uow.Object), objetos, usuarios, categorias);
    }

    [Fact]
    public async Task Proyecta_los_objetos_disponibles_del_usuario_a_DTO()
    {
        var usuarioId = Guid.NewGuid();
        var (handler, objetos, usuarios, categorias) = CrearContexto();

        var objeto = new Objeto("Bicicleta rodado 26", "En buen estado y lista para usar", 1, usuarioId, "Bueno", 1, 1, 1);
        var usuario = new Usuario("Rosa", "Quispe", "rosa@example.com", "hash", "987654321", 1, 1, 1);

        objetos.Setup(r => r.GetDisponiblesByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(new List<Objeto> { objeto });
        usuarios.Setup(r => r.GetByIdAsync(usuarioId)).ReturnsAsync(usuario);
        categorias.Setup(r => r.GetActivasAsync())
            .ReturnsAsync(new List<Categoria> { new("Deportes", "Artículos deportivos", "🚴") });

        var result = await handler.Handle(new ObtenerMisObjetosDisponiblesQuery(usuarioId), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Titulo.Should().Be("Bicicleta rodado 26");
        result[0].UsuarioNombres.Should().Be("Rosa Quispe");
        result[0].Estado.Should().Be("Disponible");
    }

    [Fact]
    public async Task Devuelve_lista_vacia_si_el_usuario_no_tiene_disponibles()
    {
        var usuarioId = Guid.NewGuid();
        var (handler, objetos, _, categorias) = CrearContexto();

        objetos.Setup(r => r.GetDisponiblesByUsuarioIdAsync(usuarioId)).ReturnsAsync(new List<Objeto>());
        categorias.Setup(r => r.GetActivasAsync()).ReturnsAsync(new List<Categoria>());

        var result = await handler.Handle(new ObtenerMisObjetosDisponiblesQuery(usuarioId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
