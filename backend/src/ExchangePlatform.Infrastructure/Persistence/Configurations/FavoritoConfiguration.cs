using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class FavoritoConfiguration : IEntityTypeConfiguration<Favorito>
{
    public void Configure(EntityTypeBuilder<Favorito> builder)
    {
        builder.ToTable("Favoritos");
        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.UsuarioId, f.ObjetoId })
               .IsUnique();

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(f => f.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Objeto>()
               .WithMany()
               .HasForeignKey(f => f.ObjetoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}