using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ObtenerRiesgo;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

public sealed class ObtenerRiesgoRepositorio(NorviaDbContext db) : IObtenerRiesgoRepositorio
{
    public Task<RiesgoResponseDto?> BuscarAsync(Guid id, CancellationToken ct) =>
        ProyeccionRiesgo.Proyectar(db, db.RiesgoEmpresa.AsNoTracking().Where(r => r.Id == id)).FirstOrDefaultAsync(ct);
}
