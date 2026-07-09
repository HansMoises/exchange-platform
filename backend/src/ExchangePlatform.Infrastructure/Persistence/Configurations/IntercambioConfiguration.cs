using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class IntercambioConfiguration : IEntityTypeConfiguration<Intercambio>
{
    public void Configure(EntityTypeBuilder<Intercambio> builder)
    {
        builder.ToTable("Intercambios");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Estado)
               .HasConversion<string>()
               .HasMaxLength(30);

        builder.Property(i => i.MensajeInicial).HasMaxLength(500);

        builder.HasOne<Objeto>()
               .WithMany()
               .HasForeignKey(i => i.ObjetoSolicitadoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Objeto>()
               .WithMany()
               .HasForeignKey(i => i.ObjetoOfrecidoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(i => i.UsuarioSolicitanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(i => i.UsuarioPropietarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.UsuarioSolicitanteId);

        builder.HasIndex(i => i.UsuarioPropietarioId);
    }
}