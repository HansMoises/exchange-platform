using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.CerrarSesion;

public class CerrarSesionCommandHandler
    : IRequestHandler<CerrarSesionCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public CerrarSesionCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(
        CerrarSesionCommand request, CancellationToken ct)
    {
        var refreshToken = await _uow.RefreshTokens
            .GetByTokenAsync(request.RefreshToken);

        if (refreshToken != null && refreshToken.EstaVigente())
        {
            refreshToken.Revocar();
            _uow.RefreshTokens.Update(refreshToken);
            await _uow.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}