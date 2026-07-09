using ExchangePlatform.Domain.Entities;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Domain;

public class AuditLogTests
{
    [Fact]
    public void Constructor_asigna_todas_las_propiedades()
    {
        var usuarioId = Guid.NewGuid();

        var log = new AuditLog(usuarioId, "Login", "Usuario", "u1", "Exitoso", "127.0.0.1", "detalle");

        log.Id.Should().NotBeEmpty();
        log.UsuarioId.Should().Be(usuarioId);
        log.Accion.Should().Be("Login");
        log.EntidadTipo.Should().Be("Usuario");
        log.EntidadId.Should().Be("u1");
        log.Resultado.Should().Be("Exitoso");
        log.IpAddress.Should().Be("127.0.0.1");
        log.Detalle.Should().Be("detalle");
    }

    [Fact]
    public void Constructor_permite_usuario_y_detalle_nulos()
    {
        var log = new AuditLog(null, "TareaProgramada", "Sistema", null, "Ok", "0.0.0.0");

        log.UsuarioId.Should().BeNull();
        log.EntidadId.Should().BeNull();
        log.Detalle.Should().BeNull();
    }
}
