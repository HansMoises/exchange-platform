namespace ExchangePlatform.Domain.Interfaces.Services;

public interface IAlmacenamientoService
{
    // Devuelve la URL publica del archivo guardado.
    Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default);
}
