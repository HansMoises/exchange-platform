using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Application.Features.Users.DTOs;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Users.Queries.ObtenerPerfil;

public class ObtenerPerfilQueryHandler
    : IRequestHandler<ObtenerPerfilQuery, PerfilUsuarioDto>
{
    private readonly IUnitOfWork _uow;

    public ObtenerPerfilQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PerfilUsuarioDto> Handle(
        ObtenerPerfilQuery request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(request.UsuarioId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        return new PerfilUsuarioDto
        {
            Id = usuario.Id,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Email = usuario.Email,
            Telefono = usuario.Telefono,
            FotoPerfil = usuario.FotoPerfil,
            RolId = usuario.RolId,
            DepartamentoId = usuario.DepartamentoId,
            ProvinciaId = usuario.ProvinciaId,
            DistritoId = usuario.DistritoId,
            CalificacionPromedio = usuario.CalificacionPromedio,
            TotalIntercambios = usuario.TotalIntercambios,
            MiembroDesde = usuario.CreatedAt
        };
    }
}