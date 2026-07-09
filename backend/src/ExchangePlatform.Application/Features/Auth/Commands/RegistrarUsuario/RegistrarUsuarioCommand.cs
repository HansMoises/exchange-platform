using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.RegistrarUsuario;

public record RegistrarUsuarioCommand(
    string Nombres,
    string Apellidos,
    string Email,
    string Password,
    string ConfirmPassword,
    string Telefono,
    int DepartamentoId,
    int ProvinciaId,
    int DistritoId) : IRequest<Guid>;