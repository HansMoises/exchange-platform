using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombres).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Apellidos).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.Telefono).IsRequired().HasMaxLength(20);
        builder.Property(u => u.FotoPerfil).HasMaxLength(500);
        builder.Property(u => u.CalificacionPromedio).HasPrecision(3, 1);
        builder.Property(u => u.Latitud).HasPrecision(10, 7);
        builder.Property(u => u.Longitud).HasPrecision(10, 7);

        builder.HasIndex(u => u.Email)
               .IsUnique();

        builder.HasOne<Domain.Entities.Maestras.Rol>()
               .WithMany()
               .HasForeignKey(u => u.RolId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}