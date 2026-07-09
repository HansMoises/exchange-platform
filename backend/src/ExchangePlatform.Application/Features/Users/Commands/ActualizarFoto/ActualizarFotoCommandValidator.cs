using FluentValidation;

namespace ExchangePlatform.Application.Features.Users.Commands.ActualizarFoto;

public class ActualizarFotoCommandValidator : AbstractValidator<ActualizarFotoCommand>
{
    public ActualizarFotoCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La url de la imagen es requerida.")
            .MaximumLength(500).WithMessage("Máximo 500 caracteres.");
    }
}
