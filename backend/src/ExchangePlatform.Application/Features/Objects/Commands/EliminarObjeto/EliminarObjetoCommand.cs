using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Commands.EliminarObjeto;

public record EliminarObjetoCommand(Guid ObjetoId, Guid UsuarioId) : IRequest<Unit>;