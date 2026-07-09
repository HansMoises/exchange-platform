using FluentValidation;

namespace ExchangePlatform.Application.Features.Reports.Commands.CrearReporte;

public class CrearReporteCommandValidator
    : AbstractValidator<CrearReporteCommand>
{
    public CrearReporteCommandValidator()
    {
        RuleFor(x => x.EntidadTipo)
            .NotEmpty()
            .Must(t => new[] { "Objeto", "Usuario" }.Contains(t))
            .WithMessage("Tipo válido: Objeto, Usuario.");

        RuleFor(x => x.EntidadId)
            .NotEmpty().WithMessage("La entidad es requerida.");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .Must(m => new[]
            {
                "ContenidoInapropiado","Fraude","Spam",
                "InformacionFalsa","Otro"
            }.Contains(m))
            .WithMessage("Motivo inválido.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("Máximo 500 caracteres.");
    }
}