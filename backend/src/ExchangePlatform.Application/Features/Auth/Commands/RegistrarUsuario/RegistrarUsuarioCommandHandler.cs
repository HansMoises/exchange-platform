using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.RegistrarUsuario;

public class RegistrarUsuarioCommandHandler
    : IRequestHandler<RegistrarUsuarioCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public RegistrarUsuarioCommandHandler(
        IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<Guid> Handle(
        RegistrarUsuarioCommand request, CancellationToken ct)
    {
        // RN-002: correo único
        if (await _uow.Usuarios.ExisteEmailAsync(request.Email))
            throw new ConflictException("El correo ya está registrado.");

        var hash = _hasher.Hash(request.Password);

        var usuario = new Usuario(
            request.Nombres,
            request.Apellidos,
            request.Email,
            hash,
            request.Telefono,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId);

        usuario.CreatedBy = Guid.Empty; // sistema

        await _uow.Usuarios.AddAsync(usuario);
        await _uow.SaveChangesAsync(ct);

        return usuario.Id;
    }
}