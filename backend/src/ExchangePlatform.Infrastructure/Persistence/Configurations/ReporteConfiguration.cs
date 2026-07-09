using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class ReporteConfiguration : IEntityTypeConfiguration<Reporte>
{
    public void Configure(EntityTypeBuilder<Reporte> builder)
    {
        builder.ToTable("Reportes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.EntidadTipo).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Motivo).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Descripcion).HasMaxLength(500);

        builder.Property(r => r.EstadoReporte)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(r => r.ReportanteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}