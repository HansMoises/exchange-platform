namespace ExchangePlatform.Application.Features.Objects.DTOs;

public class ImagenObjetoDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; }
}