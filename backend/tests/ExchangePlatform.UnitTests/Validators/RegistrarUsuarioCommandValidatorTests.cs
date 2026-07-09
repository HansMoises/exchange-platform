using ExchangePlatform.Application.Features.Auth.Commands.RegistrarUsuario;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Validators;

public class RegistrarUsuarioCommandValidatorTests
{
    private readonly RegistrarUsuarioCommandValidator _validator = new();

    private static RegistrarUsuarioCommand ComandoValido() => new(
        "Nombres", "Apellidos", "correo@example.com", "Clave123!", "Clave123!", "987654321", 1, 1, 1);

    [Fact]
    public void Comando_valido_no_genera_errores()
    {
        var resultado = _validator.Validate(ComandoValido());
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Email_con_formato_invalido_genera_error()
    {
        var comando = ComandoValido() with { Email = "no-es-un-correo" };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("corta1!")]       // menos de 8
    [InlineData("clave12345!")]   // sin mayuscula
    [InlineData("CLAVE12345!")]   // sin minuscula
    [InlineData("ClaveClave!")]   // sin numero
    [InlineData("Clave12345")]    // sin caracter especial
    public void Password_que_no_cumple_la_politica_genera_error(string password)
    {
        var comando = ComandoValido() with { Password = password, ConfirmPassword = password };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void ConfirmPassword_distinto_genera_error()
    {
        var comando = ComandoValido() with { ConfirmPassword = "OtraClave123!" };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdefghi")]
    [InlineData("")]
    public void Telefono_con_formato_invalido_genera_error(string telefono)
    {
        var comando = ComandoValido() with { Telefono = telefono };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Telefono");
    }

    [Fact]
    public void DepartamentoId_cero_genera_error()
    {
        var comando = ComandoValido() with { DepartamentoId = 0 };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DepartamentoId");
    }
}
