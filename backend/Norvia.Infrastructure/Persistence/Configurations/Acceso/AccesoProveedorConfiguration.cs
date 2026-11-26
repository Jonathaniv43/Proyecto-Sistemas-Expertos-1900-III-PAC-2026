using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class AccesoProveedorConfiguration : IEntityTypeConfiguration<AccesoProveedor>
{
    public void Configure(EntityTypeBuilder<AccesoProveedor> b)
    {
        b.HasKey(p => p.Id);
        b.EnumComoTexto(p => p.Proveedor, 20);
        b.Property(p => p.ProveedorUserId).HasMaxLength(255).IsRequired();
        b.Property(p => p.CreadoEn).FechaDelServidor();

        // RN-82: un proveedor identifica a un solo Acceso y un Acceso tiene una cuenta por proveedor.
        b.HasIndex(p => new { p.Proveedor, p.ProveedorUserId }).IsUnique();
        b.HasIndex(p => new { p.AccesoId, p.Proveedor }).IsUnique();

        b.HasOne(p => p.Acceso)
            .WithMany(a => a.Proveedores)
            .HasForeignKey(p => p.AccesoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
