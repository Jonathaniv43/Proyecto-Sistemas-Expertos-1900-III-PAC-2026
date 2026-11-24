using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.CrearControl;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa y Control son tablas de empresa (filtro global); PlantillaTarea es catálogo global.
public sealed class CrearControlRepositorio(NorviaDbContext db) : ICrearControlRepositorio
{
    public async Task<(EstadoRiesgo Estado, Guid? PeligroId)?> RiesgoAsync(Guid riesgoId, CancellationToken ct)
    {
        var r = await db.RiesgoEmpresa.AsNoTracking().Where(x => x.Id == riesgoId)
            .Select(x => new { x.Estado, x.PeligroId }).FirstOrDefaultAsync(ct);
        return r is null ? null : (r.Estado, r.PeligroId);
    }

    public Task<bool> EsSugerenciaDelPeligroAsync(Guid plantillaId, Guid peligroId, CancellationToken ct) =>
        db.PlantillaTarea.AnyAsync(p => p.Id == plantillaId && p.PeligroId == peligroId && p.Activa && p.TipoControl != null, ct);

    public Task<bool> SugerenciaYaAceptadaAsync(Guid riesgoId, Guid plantillaId, CancellationToken ct) =>
        db.Control.AnyAsync(c => c.RiesgoEmpresaId == riesgoId && c.PlantillaTareaId == plantillaId, ct);

    public void Agregar(Control control) => db.Control.Add(control);
}
