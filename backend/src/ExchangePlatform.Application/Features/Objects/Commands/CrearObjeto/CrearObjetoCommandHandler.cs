using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using MediatR;

namespace ExchangePlatform.Application.Features.Objects.Commands.CrearObjeto;

public class CrearObjetoCommandHandler
    : IRequestHandler<CrearObjetoCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CrearObjetoCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(
        CrearObjetoCommand request, CancellationToken ct)
    {
        // RN-001: usuario autenticado (garantizado por [Authorize])
        var usuario = await _uow.Usuarios.GetByIdAsync(request.UsuarioId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var objeto = new Objeto(
            request.Titulo,
            request.Descripcion,
            request.CategoriaId,
            request.UsuarioId,
            request.CondicionFisica,
            request.DepartamentoId,
            request.ProvinciaId,
            request.DistritoId);

        objeto.CreatedBy = request.UsuarioId;

        // Agregar imágenes
        int orden = 1;
        foreach (var url in request.ImagenesUrl)
        {
            var imagen = new ImagenObjeto(objeto.Id, url, orden++, 0);
            imagen.CreatedBy = request.UsuarioId;
            objeto.Imagenes.Add(imagen);
        }

        await _uow.Objetos.AddAsync(objeto);
        await _uow.SaveChangesAsync(ct);

        return objeto.Id;
    }
}