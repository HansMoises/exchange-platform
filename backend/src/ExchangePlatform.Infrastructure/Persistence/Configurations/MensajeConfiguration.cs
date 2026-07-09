using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class MensajeConfiguration : IEntityTypeConfiguration<Mensaje>
{
    public void Configure(EntityTypeBuilder<Mensaje> builder)
    {
        builder.ToTable("Mensajes");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Contenido).IsRequired().HasMaxLength(1000);

        builder.HasOne<Intercambio>()
               .WithMany()
               .HasForeignKey(m => m.IntercambioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(m => m.RemitenteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.IntercambioId);
    }
}