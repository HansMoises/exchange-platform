using ExchangePlatform.Domain.Entities.Geo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class DepartamentoConfiguration : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> builder)
    {
        builder.ToTable("Departamentos");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Ubigeo).IsRequired().HasMaxLength(2);
        builder.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.Ubigeo).IsUnique();
    }
}