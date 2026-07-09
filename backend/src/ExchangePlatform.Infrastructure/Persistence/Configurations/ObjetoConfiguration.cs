using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class ObjetoConfiguration : IEntityTypeConfiguration<Objeto>
{
    public void Configure(EntityTypeBuilder<Objeto> builder)
    {
        builder.ToTable("Objetos");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Titulo).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Descripcion).IsRequired().HasMaxLength(1000);
        builder.Property(o => o.CondicionFisica).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Latitud).HasPrecision(10, 7);
        builder.Property(o => o.Longitud).HasPrecision(10, 7);

        builder.Property(o => o.Estado)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasOne<Domain.Entities.Maestras.Categoria>()
               .WithMany()
               .HasForeignKey(o => o.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(o => o.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Imagenes)
               .WithOne()
               .HasForeignKey(i => i.ObjetoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.Estado);

        builder.HasIndex(o => o.CategoriaId);
    }
}