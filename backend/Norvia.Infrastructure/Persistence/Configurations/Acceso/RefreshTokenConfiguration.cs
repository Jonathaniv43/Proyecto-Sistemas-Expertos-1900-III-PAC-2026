using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(t => t.Id);
        b.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        b.EnumComoTexto(t => t.TipoSesion, 12);
        b.Property(t => t.ExpiraEn);
        b.Property(t => t.CreadoEn).FechaDelServidor();

        b.HasIndex(t => t.TokenHash).IsUnique();

        b.HasOne(t => t.Acceso)
            .WithMany(a => a.RefreshTokens)
            .HasForeignKey(t => t.AccesoId);

        b.HasOne(t => t.VinculoEmpresa)
            .WithMany()
            .HasForeignKey(t => t.VinculoEmpresaId);
    }
}
