using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ObtenerRiesgo;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

public sealed class ObtenerRiesgoRepositorio(NorviaDbContext db) : IObtenerRiesgoRepositorio
{
    public async Task<RiesgoResponseDto?> BuscarAsync(Guid id, CancellationToken ct) =>
        (await ProyeccionRiesgo.ListarAsync(db, db.RiesgoEmpresa.AsNoTracking().Where(r => r.Id == id), ct)).FirstOrDefault();
}
