using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Commands.ActualizarObjeto;

public record ActualizarObjetoCommand(
    Guid ObjetoId,
    Guid UsuarioId,
    string Titulo,
    string Descripcion,
    int CategoriaId,
    string CondicionFisica,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId) : IRequest<Unit>;