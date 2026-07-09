using FluentValidation;

namespace ExchangePlatform.Application.Features.Auth.Commands.RegistrarUsuario;

public class RegistrarUsuarioCommandValidator
    : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son requeridos.")
            .MinimumLength(2).WithMessage("Mínimo 2 caracteres.")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos.")
            .MinimumLength(2).WithMessage("Mínimo 2 caracteres.")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es requerido.")
            .EmailAddress().WithMessage("Formato de correo inválido.")
            .MaximumLength(256).WithMessage("Máximo 256 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("Mínimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Debe tener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("Debe tener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("Debe tener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Debe tener al menos un carácter especial.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación es requerida.")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

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