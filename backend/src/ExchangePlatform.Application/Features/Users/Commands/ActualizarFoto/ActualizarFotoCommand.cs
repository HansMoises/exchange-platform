using MediatR;

namespace ExchangePlatform.Application.Features.Users.Commands.ActualizarFoto;

public record ActualizarFotoCommand(Guid UsuarioId, string Url) : IRequest<Unit>;
