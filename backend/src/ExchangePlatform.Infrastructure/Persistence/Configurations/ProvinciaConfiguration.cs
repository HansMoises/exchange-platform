using ExchangePlatform.Domain.Entities.Geo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class ProvinciaConfiguration : IEntityTypeConfiguration<Provincia>
{
    public void Configure(EntityTypeBuilder<Provincia> builder)
    {
        builder.ToTable("Provincias");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Ubigeo).IsRequired().HasMaxLength(4);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Ubigeo).IsUnique();

        builder.HasOne(p => p.Departamento)
               .WithMany(d => d.Provincias)
               .HasForeignKey(p => p.DepartamentoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}