using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.RevisarPeligroSugerido;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

// RiesgoEmpresa es tabla de empresa: el filtro global limita a la empresa de la sesión.
public sealed class RevisarPeligroSugeridoRepositorio(NorviaDbContext db) : IRevisarPeligroSugeridoRepositorio
{
    public Task<RiesgoEmpresa?> BuscarAsync(Guid encuestaId, Guid riesgoId, CancellationToken ct) =>
        db.RiesgoEmpresa.FirstOrDefaultAsync(r => r.Id == riesgoId && r.EncuestaId == encuestaId, ct);
}
