using ExchangePlatform.Domain.Entities.Maestras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Nombre).IsUnique();

        builder.HasData(
            new { Id = 1, Nombre = "Electrónica", Descripcion = "Dispositivos electrónicos.", Icono = "📱", IsActive = true },
            new { Id = 2, Nombre = "Ropa y Accesorios", Descripcion = "Vestimenta y complementos.", Icono = "👕", IsActive = true },
            new { Id = 3, Nombre = "Hogar y Muebles", Descripcion = "Artículos para el hogar.", Icono = "🏠", IsActive = true },
            new { Id = 4, Nombre = "Juguetes y Juegos", Descripcion = "Entretenimiento infantil.", Icono = "🧸", IsActive = true },
            new { Id = 5, Nombre = "Libros y Educación", Descripcion = "Material educativo.", Icono = "📚", IsActive = true },
            new { Id = 6, Nombre = "Deportes", Descripcion = "Artículos deportivos.", Icono = "⚽", IsActive = true },
            new { Id = 7, Nombre = "Herramientas", Descripcion = "Herramientas y equipos.", Icono = "🔧", IsActive = true },
            new { Id = 8, Nombre = "Vehículos y Partes", Descripcion = "Repuestos y accesorios.", Icono = "🚗", IsActive = true },
            new { Id = 9, Nombre = "Arte y Manualidades", Descripcion = "Materiales artísticos.", Icono = "🎨", IsActive = true },
            new { Id = 10, Nombre = "Otros", Descripcion = "Otros artículos.", Icono = "📦", IsActive = true }
        );
    }
}