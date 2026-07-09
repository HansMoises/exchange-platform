using FluentValidation;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.CrearIntercambio;

public class CrearIntercambioCommandValidator : AbstractValidator<CrearIntercambioCommand>
{
    public CrearIntercambioCommandValidator()
    {
        RuleFor(x => x.ObjetoSolicitadoId)
            .NotEmpty().WithMessage("El objeto solicitado es requerido.");

        RuleFor(x => x.ObjetoOfrecidoId)
            .NotEmpty().WithMessage("El objeto ofrecido es requerido.");

        RuleFor(x => x.MensajeInicial)
            .MaximumLength(500).WithMessage("Máximo 500 caracteres.");
    }
}
