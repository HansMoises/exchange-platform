using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Commands.CrearObjeto;

public record CrearObjetoCommand(
    Guid UsuarioId,
    string Titulo,
    string Descripcion,
    int CategoriaId,
    string CondicionFisica,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId,
    List<string> ImagenesUrl) : IRequest<Guid>;