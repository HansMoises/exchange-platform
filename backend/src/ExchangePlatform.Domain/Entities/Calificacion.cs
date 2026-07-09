using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Exceptions;

namespace ExchangePlatform.Domain.Entities;

public class Calificacion : BaseEntity
{
    public Guid IntercambioId { get; private set; }
    public Guid CalificadorId { get; private set; }
    public Guid CalificadoId { get; private set; }
    public int Puntuacion { get; private set; }
    public string? Comentario { get; private set; }

    // EF Core
    protected Calificacion() { }

    public Calificacion(Guid intercambioId, Guid calificadorId,
                        Guid calificadoId, int puntuacion, string? comentario)
    {
        if (calificadorId == calificadoId)
            throw new DomainException("No puedes calificarte a ti mismo.");

        if (puntuacion < 1 || puntuacion > 5)
            throw new DomainException("La puntuación debe estar entre 1 y 5.");

        IntercambioId = intercambioId;
        CalificadorId = calificadorId;
        CalificadoId = calificadoId;
        Puntuacion = puntuacion;
        Comentario = comentario;
    }
}