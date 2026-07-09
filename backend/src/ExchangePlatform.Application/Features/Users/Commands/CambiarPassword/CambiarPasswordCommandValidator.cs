using FluentValidation;

namespace ExchangePlatform.Application.Features.Users.Commands.CambiarPassword;

public class CambiarPasswordCommandValidator
    : AbstractValidator<CambiarPasswordCommand>
{
    public CambiarPasswordCommandValidator()
    {
        RuleFor(x => x.PasswordActual)
            .NotEmpty().WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.PasswordNuevo)
            .NotEmpty().WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8).WithMessage("Mínimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Debe tener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("Debe tener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("Debe tener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Debe tener al menos un carácter especial.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación es requerida.")
            .Equal(x => x.PasswordNuevo).WithMessage("Las contraseñas no coinciden.");
    }
}