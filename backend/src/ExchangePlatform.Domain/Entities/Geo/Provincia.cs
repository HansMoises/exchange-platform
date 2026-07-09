namespace ExchangePlatform.Domain.Entities.Geo;

public class Provincia
{
    public int Id { get; private set; }
    public string Ubigeo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int DepartamentoId { get; private set; }

    // Navegación
    public Departamento Departamento { get; private set; } = null!;
    public ICollection<Distrito> Distritos { get; private set; } = new List<Distrito>();

    protected Provincia() { }
}