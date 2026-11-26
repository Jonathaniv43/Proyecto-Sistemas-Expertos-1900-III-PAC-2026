using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ListarPlantillasDocumento;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ListarPlantillasDocumentoRepositorio(NorviaDbContext db) : IListarPlantillasDocumentoRepositorio
{
    public Task<List<PlantillaDocumento>> ListarAsync(CancellationToken ct) =>
        db.PlantillaDocumento.AsNoTracking().OrderBy(p => p.TipoDocumento).ThenBy(p => p.Titulo).ToListAsync(ct);
}
