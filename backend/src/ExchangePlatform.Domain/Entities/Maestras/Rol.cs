namespace ExchangePlatform.Domain.Entities.Maestras;

public class Rol
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    protected Rol() { }
}