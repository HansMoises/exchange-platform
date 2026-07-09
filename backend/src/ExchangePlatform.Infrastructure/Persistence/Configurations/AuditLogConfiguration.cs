using ExchangePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangePlatform.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntidadTipo).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntidadId).HasMaxLength(100);
        builder.Property(a => a.Resultado).IsRequired().HasMaxLength(20);
        builder.Property(a => a.IpAddress).IsRequired().HasMaxLength(50);
        builder.HasIndex(a => a.UsuarioId);
        builder.HasIndex(a => a.OcurridoEn);
    }
}