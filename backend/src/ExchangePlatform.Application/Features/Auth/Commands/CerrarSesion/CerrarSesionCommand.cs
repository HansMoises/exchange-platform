using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.CerrarSesion;

public record CerrarSesionCommand(string RefreshToken) : IRequest<Unit>;