using ExchangePlatform.Application.Features.Objects.Commands.CrearObjeto;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Validators;

public class CrearObjetoCommandValidatorTests
{
    private readonly CrearObjetoCommandValidator _validator = new();

    private static CrearObjetoCommand ComandoValido() => new(
        Guid.NewGuid(),
        "Titulo con longitud valida",
        "Descripcion con al menos veinte caracteres de longitud.",
        1,
        "Bueno",
        1, 1, 1,
        new List<string> { "https://example.com/foto.jpg" });

    [Fact]
    public void Comando_valido_no_genera_errores()
    {
        var resultado = _validator.Validate(ComandoValido());
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Titulo_invalido_genera_error(string titulo)
    {
        var comando = ComandoValido() with { Titulo = titulo };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Titulo");
    }

    [Fact]
    public void Descripcion_muy_corta_genera_error()
    {
        var comando = ComandoValido() with { Descripcion = "muy corta" };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    [Theory]
    [InlineData("Excelente")]
    [InlineData("")]
    public void CondicionFisica_fuera_del_catalogo_permitido_genera_error(string condicion)
    {
        var comando = ComandoValido() with { CondicionFisica = condicion };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "CondicionFisica");
    }

    [Fact]
    public void Sin_imagenes_genera_error()
    {
        var comando = ComandoValido() with { ImagenesUrl = new List<string>() };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ImagenesUrl");
    }

    [Fact]
    public void Mas_de_cinco_imagenes_genera_error()
    {
        var comando = ComandoValido() with
        {
            ImagenesUrl = Enumerable.Range(1, 6).Select(i => $"https://example.com/{i}.jpg").ToList()
        };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ImagenesUrl");
    }

    [Fact]
    public void CategoriaId_cero_genera_error()
    {
        var comando = ComandoValido() with { CategoriaId = 0 };
        var resultado = _validator.Validate(comando);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "CategoriaId");
    }
}
