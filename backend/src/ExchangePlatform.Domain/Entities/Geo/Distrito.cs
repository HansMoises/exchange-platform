namespace ExchangePlatform.Domain.Entities.Geo;

public class Distrito
{
    public int Id { get; private set; }
    public string Ubigeo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int ProvinciaId { get; private set; }

    // Navegación
    public Provincia Provincia { get; private set; } = null!;

    protected Distrito() { }
}