using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Exceptions;

namespace ExchangePlatform.Domain.Entities;

public class Objeto : BaseEntity
{
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public int CategoriaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public EstadoObjeto Estado { get; private set; } = EstadoObjeto.Disponible;
    public string CondicionFisica { get; private set; } = string.Empty;
    public int DepartamentoId { get; private set; }
    public int ProvinciaId { get; private set; }
    public int DistritoId { get; private set; }
    public decimal? Latitud { get; private set; }
    public decimal? Longitud { get; private set; }

    // Navegación
    public ICollection<ImagenObjeto> Imagenes { get; private set; } = new List<ImagenObjeto>();

    // EF Core
    protected Objeto() { }

    public Objeto(string titulo, string descripcion, int categoriaId,
                  Guid usuarioId, string condicionFisica,
                  int departamentoId, int provinciaId, int distritoId)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        CategoriaId = categoriaId;
        UsuarioId = usuarioId;
        CondicionFisica = condicionFisica;
        DepartamentoId = departamentoId;
        ProvinciaId = provinciaId;
        DistritoId = distritoId;
    }

    public void Actualizar(string titulo, string descripcion,
                           int categoriaId, string condicionFisica,
                           int departamentoId, int provinciaId, int distritoId)
    {
        if (Estado != EstadoObjeto.Disponible)
            throw new DomainException("Solo se pueden editar objetos disponibles.");

        Titulo = titulo;
        Descripcion = descripcion;
        CategoriaId = categoriaId;
        CondicionFisica = condicionFisica;
        DepartamentoId = departamentoId;
        ProvinciaId = provinciaId;
        DistritoId = distritoId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reservar()
    {
        if (Estado != EstadoObjeto.Disponible)
            throw new DomainException("El objeto no está disponible para reservar.");
        Estado = EstadoObjeto.Reservado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarcarIntercambiado()
    {
        Estado = EstadoObjeto.Intercambiado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspender()
    {
        Estado = EstadoObjeto.Suspendido;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restaurar()
    {
        Estado = EstadoObjeto.Disponible;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LiberarReserva()
    {
        if (Estado == EstadoObjeto.Reservado)
        {
            Estado = EstadoObjeto.Disponible;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}