using ExchangePlatform.Domain.Interfaces.Services;
using System.Net.Http.Headers;

namespace ExchangePlatform.Infrastructure.Services;

// Sube archivos a un bucket publico de Supabase Storage via su API REST y
// devuelve la URL publica servida por el CDN de Supabase. Reemplaza a
// AlmacenamientoLocalService en Staging/Produccion (Render): el filesystem del
// contenedor es efimero y se vacia en cada deploy/reinicio, por lo que los
// archivos guardados en wwwroot/uploads se pierden aunque la BD conserve la URL.
// El HttpClient llega configurado desde Program.cs con la BaseAddress del
// proyecto Supabase y la service key como Bearer; asi este proyecto sigue
// siendo una class library plana sin dependencia del SDK de Supabase.
public class AlmacenamientoSupabaseService : IAlmacenamientoService
{
    private readonly HttpClient _http;
    private readonly string _bucket;

    public AlmacenamientoSupabaseService(HttpClient http, string bucket)
    {
        _http = http;
        _bucket = bucket;
    }

    public async Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default)
    {
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"storage/v1/object/{_bucket}/{nombreArchivo}");
        request.Content = new StreamContent(contenido);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(MimePorExtension(extension));

        using var respuesta = await _http.SendAsync(request, ct);
        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Supabase Storage rechazó la subida ({(int)respuesta.StatusCode}): {detalle}");
        }

        return $"{_http.BaseAddress}storage/v1/object/public/{_bucket}/{nombreArchivo}";
    }

    // Los controllers solo aceptan JPEG/PNG/WEBP; cualquier otra extension cae
    // al generico para no romper si esa lista cambia antes que esta.
    private static string MimePorExtension(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}
