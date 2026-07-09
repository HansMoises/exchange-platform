using MediatR;

namespace ExchangePlatform.Application.Features.Users.Commands.CambiarPassword;

public record CambiarPasswordCommand(
    Guid UsuarioId,
    string PasswordActual,
    string PasswordNuevo,
    string ConfirmPassword) : IRequest<Unit>;