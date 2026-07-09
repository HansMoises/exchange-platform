using MediatR;

namespace ExchangePlatform.Application.Features.Users.Commands.ActualizarPerfil;

public record ActualizarPerfilCommand(
    Guid UsuarioId,
    string Nombres,
    string Apellidos,
    string Telefono,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId) : IRequest<Unit>;