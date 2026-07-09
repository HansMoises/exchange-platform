namespace ExchangePlatform.Domain.Entities.Maestras;

public class Categoria
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string? Icono { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected Categoria() { }

    public Categoria(string nombre, string? descripcion, string? icono)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        Icono = icono;
    }

    public void Actualizar(string nombre, string? descripcion, string? icono)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        Icono = icono;
    }

    public void Activar() => IsActive = true;

    public void Desactivar() => IsActive = false;
}