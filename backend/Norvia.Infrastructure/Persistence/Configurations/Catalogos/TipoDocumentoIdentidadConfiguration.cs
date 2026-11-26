using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class TipoDocumentoIdentidadConfiguration : IEntityTypeConfiguration<TipoDocumentoIdentidadCatalogo>
{
    public void Configure(EntityTypeBuilder<TipoDocumentoIdentidadCatalogo> b)
    {
        b.ToTable("TipoDocumentoIdentidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Codigo).HasMaxLength(15).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}