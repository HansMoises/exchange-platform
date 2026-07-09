using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Application.Features.Exchanges.DTOs;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Exchanges.Commands.ConfirmarIntercambio;

public class ConfirmarIntercambioCommandHandler
    : IRequestHandler<ConfirmarIntercambioCommand, IntercambioDto>
{
    private readonly IUnitOfWork _uow;

    public ConfirmarIntercambioCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IntercambioDto> Handle(
        ConfirmarIntercambioCommand request, CancellationToken ct)
    {
        var intercambio = await _uow.Intercambios
            .GetByIdAsync(request.IntercambioId)
            ?? throw new NotFoundException("Intercambio no encontrado.");

        // Verificar que el usuario es parte del intercambio
        if (intercambio.UsuarioSolicitanteId != request.UsuarioId &&
            intercambio.UsuarioPropietarioId != request.UsuarioId)
            throw new ForbiddenException("No eres parte de este intercambio.");

        intercambio.Confirmar(request.UsuarioId);
        intercambio.UpdatedBy = request.UsuarioId;

        // Si completado: marcar objetos como intercambiados
        if (intercambio.Estado == EstadoIntercambio.Completado)
        {
            var objetoSolicitado = await _uow.Objetos
                .GetByIdAsync(intercambio.ObjetoSolicitadoId);
            var objetoOfrecido = await _uow.Objetos
                .GetByIdAsync(intercambio.ObjetoOfrecidoId);

            objetoSolicitado?.MarcarIntercambiado();
            objetoOfrecido?.MarcarIntercambiado();

            // Incrementar contadores de intercambio
            var solicitante = await _uow.Usuarios
                .GetByIdAsync(intercambio.UsuarioSolicitanteId);
            var propietario = await _uow.Usuarios
                .GetByIdAsync(intercambio.UsuarioPropietarioId);

            solicitante?.IncrementarIntercambios();
            propietario?.IncrementarIntercambios();

            if (objetoSolicitado != null) _uow.Objetos.Update(objetoSolicitado);
            if (objetoOfrecido != null) _uow.Objetos.Update(objetoOfrecido);
            if (solicitante != null) _uow.Usuarios.Update(solicitante);
            if (propietario != null) _uow.Usuarios.Update(propietario);
        }

        _uow.Intercambios.Update(intercambio);
        await _uow.SaveChangesAsync(ct);

        return new IntercambioDto
        {
            Id = intercambio.Id,
            ObjetoSolicitadoId = intercambio.ObjetoSolicitadoId,
            ObjetoOfrecidoId = intercambio.ObjetoOfrecidoId,
            UsuarioSolicitanteId = intercambio.UsuarioSolicitanteId,
            UsuarioPropietarioId = intercambio.UsuarioPropietarioId,
            Estado = intercambio.Estado.ToString(),
            ConfirmacionSolicitante = intercambio.ConfirmacionSolicitante,
            ConfirmacionPropietario = intercambio.ConfirmacionPropietario,
            FechaAceptacion = intercambio.FechaAceptacion,
            FechaCompletado = intercambio.FechaCompletado,
            CreadoEn = intercambio.CreatedAt
        };
    }
}