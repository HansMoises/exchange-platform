using MediatR;

namespace ExchangePlatform.Application.Features.Ratings.Commands.CrearCalificacion;

public record CrearCalificacionCommand(
    Guid IntercambioId,
    Guid CalificadorId,
    int Puntuacion,
    string? Comentario) : IRequest<Guid>;