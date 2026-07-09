using FluentValidation;

namespace ExchangePlatform.Application.Features.Auth.Commands.OlvidarPassword;

public class OlvidarPasswordCommandValidator : AbstractValidator<OlvidarPasswordCommand>
{
    public OlvidarPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es requerido.")
            .EmailAddress().WithMessage("Formato de correo inválido.");
    }
}
