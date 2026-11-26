using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class CambiarEstadoPlantillaDocumentoRepositorio(NorviaDbContext db) : ICambiarEstadoPlantillaDocumentoRepositorio
{
    public Task<PlantillaDocumento?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.PlantillaDocumento.FirstOrDefaultAsync(e => e.Id == id, ct);
}
