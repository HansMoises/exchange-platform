using FluentValidation;

namespace ExchangePlatform.Application.Features.Ratings.Commands.CrearCalificacion;

public class CrearCalificacionCommandValidator
    : AbstractValidator<CrearCalificacionCommand>
{
    public CrearCalificacionCommandValidator()
    {
        RuleFor(x => x.IntercambioId)
            .NotEmpty().WithMessage("El intercambio es requerido.");

        RuleFor(x => x.Puntuacion)
            .InclusiveBetween(1, 5)
            .WithMessage("La puntuación debe estar entre 1 y 5.");

        RuleFor(x => x.Comentario)
            .MaximumLength(500).WithMessage("Máximo 500 caracteres.");
    }
}