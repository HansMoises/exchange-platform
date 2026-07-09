using ExchangePlatform.Application.Features.Objects.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerObjetoPorId;

public record ObtenerObjetoPorIdQuery(Guid ObjetoId) : IRequest<ObjetoDto>;