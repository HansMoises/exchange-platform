using ExchangePlatform.Domain.Entities.Maestras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.Nombre).IsUnique();

        builder.HasData(
            new { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema." },
            new { Id = 2, Nombre = "Moderador", Descripcion = "Gestión de reportes y contenido." },
            new { Id = 3, Nombre = "Usuario", Descripcion = "Usuario estándar de la plataforma." }
        );
    }
}