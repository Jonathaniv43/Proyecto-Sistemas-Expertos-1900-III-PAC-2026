using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Empresas;

public class HistorialEstadoEmpresaConfiguration : IEntityTypeConfiguration<HistorialEstadoEmpresa>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoEmpresa> b)
    {
        b.HasKey(h => h.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(h => h.EstadoAnterior, 12);
        b.EnumComoTexto(h => h.EstadoNuevo, 12);
        b.EnumComoTexto(h => h.MotivoCodigo, 30);
        b.Property(h => h.MotivoDetalle);
        b.Property(h => h.RegistradoEn).FechaDelServidor();

        b.HasOne(h => h.Empresa).WithMany(e => e.HistorialEstados).HasForeignKey(h => h.EmpresaId);

        // StaffPlataforma es global: FK simple. Nulo si el cambio fue automático.
        b.HasOne(h => h.RegistradoPor).WithMany().HasForeignKey(h => h.RegistradoPorId);
    }
}
