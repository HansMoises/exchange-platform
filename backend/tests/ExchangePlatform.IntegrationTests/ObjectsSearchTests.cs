using System.Net;
using System.Net.Http.Headers;
using ExchangePlatform.Domain.Common;
using FluentAssertions;

namespace ExchangePlatform.IntegrationTests;

// Ejercita los filtros y ordenamientos de ObjetoRepository (GetDisponiblesAsync,
// CountDisponiblesAsync y AplicarOrden) que la búsqueda por defecto no cubre.
public class ObjectsSearchTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ObjectsSearchTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Busqueda_con_filtros_y_ordenamientos_responde_200()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestAuthHelper.CrearUsuarioConRolAsync(_factory, client, RolesConocidos.Usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await TestObjectHelper.CrearObjetoAsync(client, "Objeto para busqueda filtrada valido");

        var anon = _factory.CreateClient();
        var consultas = new[]
        {
            "/api/v1/objects?search=Objeto&categoriaId=1&departamentoId=1&provinciaId=1&distritoId=1",
            "/api/v1/objects?sortBy=titulo&sortOrder=asc",
            "/api/v1/objects?sortBy=titulo&sortOrder=desc",
            "/api/v1/objects?sortBy=calificacionPromedio&sortOrder=asc",
            "/api/v1/objects?sortBy=calificacionPromedio&sortOrder=desc",
        };

        foreach (var url in consultas)
        {
            var respuesta = await anon.GetAsync(url);
            respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
