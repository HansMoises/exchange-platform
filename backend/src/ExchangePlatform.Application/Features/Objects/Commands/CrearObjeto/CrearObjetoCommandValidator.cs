using FluentValidation;

namespace ExchangePlatform.Application.Features.Objects.Commands.CrearObjeto;

public class CrearObjetoCommandValidator
    : AbstractValidator<CrearObjetoCommand>
{
    public CrearObjetoCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es requerido.")
            .MinimumLength(5).WithMessage("Mínimo 5 caracteres.")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MinimumLength(20).WithMessage("Mínimo 20 caracteres.")
            .MaximumLength(1000).WithMessage("Máximo 1000 caracteres.");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0).WithMessage("La categoría es requerida.");

        RuleFor(x => x.CondicionFisica)
            .NotEmpty().WithMessage("La condición física es requerida.")
            .Must(c => new[] { "Nuevo", "Bueno", "Regular" }.Contains(c))
            .WithMessage("Condición válida: Nuevo, Bueno, Regular.");

        RuleFor(x => x.DepartamentoId)
            .GreaterThan(0).WithMessage("El departamento es requerido.");

        RuleFor(x => x.ProvinciaId)
            .GreaterThan(0).WithMessage("La provincia es requerida.");

        RuleFor(x => x.DistritoId)
            .GreaterThan(0).WithMessage("El distrito es requerido.");

        RuleFor(x => x.ImagenesUrl)
            .NotNull().WithMessage("Debe incluir al menos una imagen.")
            .Must(i => i.Count >= 1).WithMessage("Mínimo 1 imagen.")
            .Must(i => i.Count <= 5).WithMessage("Máximo 5 imágenes.");
    }
}