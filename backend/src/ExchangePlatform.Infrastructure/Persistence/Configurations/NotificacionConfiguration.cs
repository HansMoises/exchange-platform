using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Tipo)
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(n => n.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Mensaje).IsRequired().HasMaxLength(500);
        builder.Property(n => n.EntidadTipo).HasMaxLength(50);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(n => n.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new { n.UsuarioId, n.IsLeida });
    }
}