using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Exceptions;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Domain;

public class ObjetoTests
{
    private static Objeto CrearObjetoValido() =>
        new("Titulo valido", "Descripcion valida", 1, Guid.NewGuid(), "Bueno", 1, 1, 1);

    [Fact]
    public void Objeto_nuevo_empieza_Disponible()
    {
        var objeto = CrearObjetoValido();
        objeto.Estado.Should().Be(EstadoObjeto.Disponible);
    }

    [Fact]
    public void Reservar_cambia_a_Reservado()
    {
        var objeto = CrearObjetoValido();
        objeto.Reservar();
        objeto.Estado.Should().Be(EstadoObjeto.Reservado);
    }

    [Fact]
    public void Reservar_lanza_si_no_esta_Disponible()
    {
        var objeto = CrearObjetoValido();
        objeto.Reservar();

        var accion = () => objeto.Reservar();

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void LiberarReserva_vuelve_a_Disponible_si_estaba_Reservado()
    {
        var objeto = CrearObjetoValido();
        objeto.Reservar();

        objeto.LiberarReserva();

        objeto.Estado.Should().Be(EstadoObjeto.Disponible);
    }

    [Fact]
    public void LiberarReserva_no_cambia_nada_si_no_estaba_Reservado()
    {
        var objeto = CrearObjetoValido();

        objeto.LiberarReserva();

        objeto.Estado.Should().Be(EstadoObjeto.Disponible);
    }

    [Fact]
    public void Actualizar_lanza_si_el_objeto_no_esta_Disponible()
    {
        var objeto = CrearObjetoValido();
        objeto.Reservar();

        var accion = () => objeto.Actualizar("Nuevo titulo", "Nueva descripcion", 1, "Bueno", 1, 1, 1);

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Actualizar_funciona_si_esta_Disponible()
    {
        var objeto = CrearObjetoValido();

        objeto.Actualizar("Nuevo titulo", "Nueva descripcion", 2, "Nuevo", 2, 2, 2);

        objeto.Titulo.Should().Be("Nuevo titulo");
        objeto.CategoriaId.Should().Be(2);
        objeto.CondicionFisica.Should().Be("Nuevo");
    }

    [Fact]
    public void Suspender_y_Restaurar_alternan_el_estado()
    {
        var objeto = CrearObjetoValido();

        objeto.Suspender();
        objeto.Estado.Should().Be(EstadoObjeto.Suspendido);

        objeto.Restaurar();
        objeto.Estado.Should().Be(EstadoObjeto.Disponible);
    }

    [Fact]
    public void MarcarIntercambiado_cambia_el_estado_sin_importar_el_estado_previo()
    {
        var objeto = CrearObjetoValido();
        objeto.Reservar();

        objeto.MarcarIntercambiado();

        objeto.Estado.Should().Be(EstadoObjeto.Intercambiado);
    }
}
