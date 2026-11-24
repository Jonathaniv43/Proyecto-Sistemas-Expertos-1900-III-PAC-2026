using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.CrearRiesgo;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa es tabla de empresa: el filtro global limita a la empresa de la sesión.
public sealed class CrearRiesgoRepositorio(NorviaDbContext db) : ICrearRiesgoRepositorio
{
    public Task<string?> NombrePeligroActivoAsync(Guid peligroId, CancellationToken ct) =>
        db.Peligro.AsNoTracking().Where(p => p.Id == peligroId && p.Activo).Select(p => p.Nombre).FirstOrDefaultAsync(ct);

    public Task<bool> ExisteEnMatrizAsync(Guid? peligroId, string? descripcionPeligro, string actividad, CancellationToken ct)
    {
        var actividadMin = actividad.ToLower();
        var enMatriz = db.RiesgoEmpresa.Where(r => r.Estado == EstadoRiesgo.Confirmado && r.Actividad != null && r.Actividad.ToLower() == actividadMin);
        if (peligroId is not null) return enMatriz.AnyAsync(r => r.PeligroId == peligroId, ct);
        var descripcionMin = descripcionPeligro!.ToLower();
        return enMatriz.AnyAsync(r => r.PeligroId == null && r.DescripcionPeligro!.ToLower() == descripcionMin, ct);
    }

    public void Agregar(RiesgoEmpresa riesgo) => db.RiesgoEmpresa.Add(riesgo);
}
