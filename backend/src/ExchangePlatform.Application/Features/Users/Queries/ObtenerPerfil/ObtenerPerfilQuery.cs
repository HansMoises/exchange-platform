using ExchangePlatform.Application.Features.Users.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Users.Queries.ObtenerPerfil;

public record ObtenerPerfilQuery(Guid UsuarioId) : IRequest<PerfilUsuarioDto>;