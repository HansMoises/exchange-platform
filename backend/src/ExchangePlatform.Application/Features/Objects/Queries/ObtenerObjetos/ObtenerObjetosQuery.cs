using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Objects.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Queries.ObtenerObjetos;

public record ObtenerObjetosQuery(
    string? Search,
    int? CategoriaId,
    int? DepartamentoId,
    int? ProvinciaId,
    int? DistritoId,
    string? SortBy = null,
    string? SortOrder = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ObjetoDto>>;