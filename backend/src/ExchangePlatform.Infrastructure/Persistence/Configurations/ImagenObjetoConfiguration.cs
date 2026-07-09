using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class ImagenObjetoConfiguration : IEntityTypeConfiguration<ImagenObjeto>
{
    public void Configure(EntityTypeBuilder<ImagenObjeto> builder)
    {
        builder.ToTable("ImagenesObjeto");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Url).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Orden).IsRequired();
        builder.Property(i => i.TamanoKb).IsRequired();
    }
}