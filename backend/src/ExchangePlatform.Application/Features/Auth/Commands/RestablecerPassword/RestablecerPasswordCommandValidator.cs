using FluentValidation;

namespace ExchangePlatform.Application.Features.Auth.Commands.RestablecerPassword;

public class RestablecerPasswordCommandValidator
    : AbstractValidator<RestablecerPasswordCommand>
{
    public RestablecerPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token es requerido.");

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
    }
}
