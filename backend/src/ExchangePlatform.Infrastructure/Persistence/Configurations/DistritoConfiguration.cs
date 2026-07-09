using ExchangePlatform.Domain.Entities.Geo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class DistritoConfiguration : IEntityTypeConfiguration<Distrito>
{
    public void Configure(EntityTypeBuilder<Distrito> builder)
    {
        builder.ToTable("Distritos");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Ubigeo).IsRequired().HasMaxLength(6);
        builder.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.Ubigeo).IsUnique();

        builder.HasOne(d => d.Provincia)
               .WithMany(p => p.Distritos)
               .HasForeignKey(d => d.ProvinciaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}