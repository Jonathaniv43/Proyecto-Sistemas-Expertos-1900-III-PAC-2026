using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.ListarPlantillasDisponibles;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// Empresa no lleva filtro global (es la raíz): se busca por el EmpresaId de la sesión. PlantillaDocumento es global.
public sealed class ListarPlantillasDisponiblesRepositorio(NorviaDbContext db) : IListarPlantillasDisponiblesRepositorio
{
    public Task<short?> RubroDeEmpresaAsync(Guid empresaId, CancellationToken ct) =>
        db.Empresa.AsNoTracking().Where(e => e.Id == empresaId).Select(e => e.RubroId).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PlantillaDisponibleResponseDto>> ListarAsync(short rubroId, CancellationToken ct)
    {
        var plantillas = await db.PlantillaDocumento.AsNoTracking()
            .Where(p => p.Activa && (p.RubroId == null || p.RubroId == rubroId))
            .Select(p => new PlantillaDisponibleResponseDto(p.Id, p.Codigo, p.Titulo, p.TipoDocumento))
            .ToListAsync(ct);
        return plantillas.OrderBy(p => p.TipoDocumento).ThenBy(p => p.Titulo, StringComparer.CurrentCulture).ToList();
    }
}
