using System.Net.Http.Json;
using System.Text.Json;
using ExchangePlatform.API.Controllers;
using ExchangePlatform.Application.Common.Models;

namespace ExchangePlatform.IntegrationTests;

public static class TestObjectHelper
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);

    public static async Task<Guid> CrearObjetoAsync(HttpClient clienteAutenticado, string titulo = "Objeto de prueba valido")
    {
        var request = new CrearObjetoRequest(
            Titulo: titulo,
            Descripcion: "Descripcion de prueba con al menos veinte caracteres.",
            CategoriaId: 1,
            CondicionFisica: "Bueno",
            DepartamentoId: 1,
            ProvinciaId: 1,
            DistritoId: 1,
            ImagenesUrl: new List<string> { "https://example.com/imagen.jpg" });

        var respuesta = await clienteAutenticado.PostAsJsonAsync("/api/v1/objects", request);
        respuesta.EnsureSuccessStatusCode();

        var contenido = await respuesta.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(JsonOpciones);
        return contenido!.Data.GetProperty("id").GetGuid();
    }
}
