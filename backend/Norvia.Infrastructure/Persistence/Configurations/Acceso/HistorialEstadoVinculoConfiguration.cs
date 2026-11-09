using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class HistorialEstadoVinculoConfiguration : IEntityTypeConfiguration<HistorialEstadoVinculo>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoVinculo> b)
    {
        b.HasKey(h => h.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(h => h.EstadoAnterior, 12); // nulo en el primer registro
        b.EnumComoTexto(h => h.EstadoNuevo, 12);
        b.EnumComoTexto(h => h.MotivoCodigo, 30);
        b.Property(h => h.MotivoDetalle);
        b.Property(h => h.RegistradoEn).FechaDelServidor();

        b.HasOne<Empresa>().WithMany().HasForeignKey(h => h.EmpresaId);

        // Dos FK compuestas hacia VinculoEmpresa: el vínculo afectado y quien registró el cambio.
        b.HasOne(h => h.VinculoEmpresa)
            .WithMany(v => v.HistorialEstados)
            .HasForeignKey(h => new { h.EmpresaId, h.VinculoEmpresaId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasOne(h => h.RegistradoPor)
            .WithMany()
            .HasForeignKey(h => new { h.EmpresaId, h.RegistradoPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
    }
}
