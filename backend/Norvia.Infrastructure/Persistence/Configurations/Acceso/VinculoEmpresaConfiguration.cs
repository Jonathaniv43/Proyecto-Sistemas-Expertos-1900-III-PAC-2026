using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class VinculoEmpresaConfiguration : IEntityTypeConfiguration<VinculoEmpresa>
{
    public void Configure(EntityTypeBuilder<VinculoEmpresa> b)
    {
        b.HasKey(v => v.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(v => v.Estado, 12);
        b.Property(v => v.CreadoEn).FechaDelServidor();

        b.HasOne(v => v.Empresa).WithMany(e => e.Vinculos).HasForeignKey(v => v.EmpresaId);
        b.HasOne(v => v.Persona).WithMany(p => p.Vinculos).HasForeignKey(v => v.PersonaId);
        b.HasOne(v => v.Rol).WithMany(r => r.Vinculos).HasForeignKey(v => v.RolId);
        b.HasOne(v => v.Acceso).WithMany(a => a.Vinculos).HasForeignKey(v => v.AccesoId);

        b.HasIndex(v => new { v.EmpresaId, v.PersonaId }).IsUnique();
        b.HasIndex(v => new { v.EmpresaId, v.AccesoId }).IsUnique();
    }
}
