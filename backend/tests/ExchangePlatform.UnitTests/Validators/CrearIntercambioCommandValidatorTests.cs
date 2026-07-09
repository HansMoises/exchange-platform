using ExchangePlatform.Application.Features.Exchanges.Commands.CrearIntercambio;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Validators;

public class CrearIntercambioCommandValidatorTests
{
    private readonly CrearIntercambioCommandValidator _validator = new();

    private static CrearIntercambioCommand ComandoValido() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Hola, me interesa tu objeto.");

    [Fact]
    public void Comando_valido_no_genera_errores()
    {
        var resultado = _validator.Validate(ComandoValido());
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ObjetoSolicitadoId_vacio_genera_error()
    {
        var comando = ComandoValido() with { ObjetoSolicitadoId = Guid.Empty };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ObjetoSolicitadoId");
    }

    [Fact]
    public void ObjetoOfrecidoId_vacio_genera_error()
    {
        var comando = ComandoValido() with { ObjetoOfrecidoId = Guid.Empty };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ObjetoOfrecidoId");
    }

    [Fact]
    public void MensajeInicial_muy_largo_genera_error()
    {
        var comando = ComandoValido() with { MensajeInicial = new string('a', 501) };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "MensajeInicial");
    }

    [Fact]
    public void MensajeInicial_nulo_es_valido()
    {
        var comando = ComandoValido() with { MensajeInicial = null };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeTrue();
    }
}
