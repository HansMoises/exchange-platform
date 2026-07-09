using ExchangePlatform.Domain.Common;

namespace ExchangePlatform.Domain.Entities;

public class ImagenObjeto : BaseEntity
{
    public Guid ObjetoId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int Orden { get; private set; }
    public int TamanoKb { get; private set; }

    // EF Core
    protected ImagenObjeto() { }

    public ImagenObjeto(Guid objetoId, string url, int orden, int tamanoKb)
    {
        ObjetoId = objetoId;
        Url = url;
        Orden = orden;
        TamanoKb = tamanoKb;
    }
}