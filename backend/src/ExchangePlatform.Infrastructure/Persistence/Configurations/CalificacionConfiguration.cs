using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class CalificacionConfiguration : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> builder)
    {
        builder.ToTable("Calificaciones");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Puntuacion).IsRequired();
        builder.Property(c => c.Comentario).HasMaxLength(500);

        builder.HasIndex(c => new { c.IntercambioId, c.CalificadorId })
               .IsUnique();

        builder.HasOne<Intercambio>()
               .WithMany()
               .HasForeignKey(c => c.IntercambioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(c => c.CalificadorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(c => c.CalificadoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}