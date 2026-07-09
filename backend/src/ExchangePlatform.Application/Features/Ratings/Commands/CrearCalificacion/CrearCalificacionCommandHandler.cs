using AppValidationException = ExchangePlatform.Application.Common.Exceptions.ValidationException;
using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Ratings.Commands.CrearCalificacion;

public class CrearCalificacionCommandHandler
    : IRequestHandler<CrearCalificacionCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CrearCalificacionCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(
        CrearCalificacionCommand request, CancellationToken ct)
    {
        var intercambio = await _uow.Intercambios
            .GetByIdAsync(request.IntercambioId)
            ?? throw new NotFoundException("Intercambio no encontrado.");

        // Solo se puede calificar intercambios completados
        if (intercambio.Estado != EstadoIntercambio.Completado)
            throw new AppValidationException(
                new List<string> { "Solo puedes calificar intercambios completados." });

        // Verificar que el calificador es parte del intercambio
        if (intercambio.UsuarioSolicitanteId != request.CalificadorId &&
            intercambio.UsuarioPropietarioId != request.CalificadorId)
            throw new ForbiddenException("No participaste en este intercambio.");

        // Determinar quién es el calificado
        var calificadoId = intercambio.UsuarioSolicitanteId == request.CalificadorId
            ? intercambio.UsuarioPropietarioId
            : intercambio.UsuarioSolicitanteId;

        var calificacion = new Calificacion(
            request.IntercambioId,
            request.CalificadorId,
            calificadoId,
            request.Puntuacion,
            request.Comentario);

        calificacion.CreatedBy = request.CalificadorId;

        await _uow.Calificaciones.AddAsync(calificacion);

        // Recalcular reputación del calificado (RN-032)
        var calificado = await _uow.Usuarios.GetByIdAsync(calificadoId);
        if (calificado != null)
        {
            var todasCalificaciones = await _uow.Calificaciones
                .GetByCalificadoIdAsync(calificadoId);

            var totalPuntuacion = todasCalificaciones.Sum(c => c.Puntuacion)
                + request.Puntuacion;
            var totalCount = todasCalificaciones.Count() + 1;
            var promedio = Math.Round((decimal)totalPuntuacion / totalCount, 1);

            calificado.ActualizarReputacion(promedio);
            calificado.UpdatedBy = request.CalificadorId;
            _uow.Usuarios.Update(calificado);
        }

        await _uow.SaveChangesAsync(ct);
        return calificacion.Id;
    }
}