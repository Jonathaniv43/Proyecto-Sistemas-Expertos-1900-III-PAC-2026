using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Riesgos;

public class ControlConfiguration : IEntityTypeConfiguration<Control>
{
    public void Configure(EntityTypeBuilder<Control> b)
    {
        b.HasKey(c => c.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(c => c.Tipo, 15);
        b.Property(c => c.Descripcion).IsRequired();
        b.EnumComoTexto(c => c.Estado, 12);
        b.Property(c => c.CreadoEn).FechaDelServidor();

        b.HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId);

        b.HasOne(c => c.RiesgoEmpresa)
            .WithMany(r => r.Controles)
            .HasForeignKey(c => new { c.EmpresaId, c.RiesgoEmpresaId })
            .HasPrincipalKey(r => new { r.EmpresaId, r.Id });

        // UQ (RiesgoEmpresaId, Id): Tarea lo usará para verificar que su control es del mismo riesgo.
        b.HasAlternateKey(c => new { c.RiesgoEmpresaId, c.Id });
    }
}
