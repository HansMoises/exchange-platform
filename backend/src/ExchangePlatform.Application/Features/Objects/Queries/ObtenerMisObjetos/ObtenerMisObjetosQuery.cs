using ExchangePlatform.Application.Features.Objects.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerMisObjetos;

public record ObtenerMisObjetosQuery(Guid UsuarioId) : IRequest<List<ObjetoDto>>;