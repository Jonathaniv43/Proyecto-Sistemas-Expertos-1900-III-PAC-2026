using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.ListarControlesSugeridos;
using Norvia.Application.Features.Riesgos.ListarControlesSugeridos.DTOs;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa y Control son tablas de empresa (filtro global); PlantillaTarea es catálogo global.
public sealed class ListarControlesSugeridosRepositorio(NorviaDbContext db) : IListarControlesSugeridosRepositorio
{
    public async Task<(Guid? PeligroId, bool Existe)> PeligroDelRiesgoAsync(Guid riesgoId, CancellationToken ct)
    {
        var r = await db.RiesgoEmpresa.AsNoTracking().Where(x => x.Id == riesgoId).Select(x => new { x.PeligroId }).FirstOrDefaultAsync(ct);
        return (r?.PeligroId, r is not null);
    }

    public async Task<IReadOnlyList<ControlSugeridoResponseDto>> PendientesAsync(Guid riesgoId, Guid peligroId, CancellationToken ct)
    {
        var plantillas = await db.PlantillaTarea.AsNoTracking()
            .Where(p => p.PeligroId == peligroId && p.Activa && p.TipoControl != null)
            .Where(p => !db.Control.Any(c => c.RiesgoEmpresaId == riesgoId && c.PlantillaTareaId == p.Id))
            .Select(p => new { p.Id, p.TipoControl, p.Titulo, p.Descripcion })
            .ToListAsync(ct);
        return plantillas.Select(p => new ControlSugeridoResponseDto(p.Id, p.TipoControl!.Value, p.Titulo, p.Descripcion)).ToList();
    }
}
