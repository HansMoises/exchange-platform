namespace ExchangePlatform.Domain.Entities.Geo;

public class Departamento
{
    public int Id { get; private set; }
    public string Ubigeo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;

    // Navegación
    public ICollection<Provincia> Provincias { get; private set; } = new List<Provincia>();

    protected Departamento() { }
}