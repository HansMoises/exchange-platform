using ExchangePlatform.Domain.Interfaces.Services;

namespace ExchangePlatform.Infrastructure.Services;

// Guarda archivos en disco (carpeta wwwroot/uploads del API) y devuelve una
// URL publica servida como archivo estatico. La ruta y la URL base las
// resuelve el composition root (Program.cs), que es quien conoce el
// IWebHostEnvironment; este proyecto es una class library plana y no
// referencia ASP.NET Core directamente. Reemplazar por Azure Blob/S3/etc.
// cuando se decida un proveedor de almacenamiento en la nube.
public class AlmacenamientoLocalService : IAlmacenamientoService
{
    private readonly string _rutaUploads;
    private readonly string _baseUrl;

    public AlmacenamientoLocalService(string rutaUploads, string baseUrl)
    {
        _rutaUploads = rutaUploads;
        _baseUrl = baseUrl.TrimEnd('/');
        Directory.CreateDirectory(_rutaUploads);
    }

    public async Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default)
    {
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(_rutaUploads, nombreArchivo);

        await using var destino = File.Create(rutaCompleta);
        await contenido.CopyToAsync(destino, ct);

        return $"{_baseUrl}/uploads/{nombreArchivo}";
    }
}
