using FluentValidation;

namespace ExchangePlatform.Application.Features.Users.Commands.ActualizarPerfil;

public class ActualizarPerfilCommandValidator
    : AbstractValidator<ActualizarPerfilCommand>
{
    public ActualizarPerfilCommandValidator()
    {
        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son requeridos.")
            .MinimumLength(2).WithMessage("Mínimo 2 caracteres.")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos.")
            .MinimumLength(2).WithMessage("Mínimo 2 caracteres.")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres.");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es requerido.")
            .Matches(@"^[0-9]{9}$").WithMessage("El teléfono debe tener 9 dígitos.");

        RuleFor(x => x.DepartamentoId)
            .GreaterThan(0).WithMessage("El departamento es requerido.");

        RuleFor(x => x.ProvinciaId)
            .GreaterThan(0).WithMessage("La provincia es requerida.");

        RuleFor(x => x.DistritoId)
            .GreaterThan(0).WithMessage("El distrito es requerido.");
    }
}