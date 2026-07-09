namespace ExchangePlatform.Application.Features.Objects.DTOs;

public class ObjetoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string UsuarioNombres { get; set; } = string.Empty;
    public decimal UsuarioCalificacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string CondicionFisica { get; set; } = string.Empty;
    public int DepartamentoId { get; set; }
    public int ProvinciaId { get; set; }
    public int DistritoId { get; set; }
    public List<ImagenObjetoDto> Imagenes { get; set; } = new();
    public DateTime CreadoEn { get; set; }
}