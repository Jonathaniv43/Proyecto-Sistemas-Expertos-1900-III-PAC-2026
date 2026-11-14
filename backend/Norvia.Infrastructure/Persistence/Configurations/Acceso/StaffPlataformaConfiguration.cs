using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class StaffPlataformaConfiguration : IEntityTypeConfiguration<StaffPlataforma>
{
    public void Configure(EntityTypeBuilder<StaffPlataforma> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.Activo).PorDefectoTrue();
        b.Property(s => s.CreadoEn).FechaDelServidor();

        b.HasOne(s => s.RolStaffPlataforma)
            .WithMany(r => r.Staff)
            .HasForeignKey(s => s.RolStaffPlataformaId);

        b.HasOne(s => s.Acceso)
            .WithOne(a => a.Staff)
            .HasForeignKey<StaffPlataforma>(s => s.AccesoId);

        b.HasIndex(s => s.AccesoId).IsUnique();
    }
}
