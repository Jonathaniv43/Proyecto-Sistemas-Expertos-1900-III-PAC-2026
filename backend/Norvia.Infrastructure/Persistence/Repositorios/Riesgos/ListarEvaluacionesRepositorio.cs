using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarEvaluaciones;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa, EvaluacionRiesgo y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class ListarEvaluacionesRepositorio(NorviaDbContext db) : IListarEvaluacionesRepositorio
{
    public Task<bool> ExisteRiesgoAsync(Guid riesgoId, CancellationToken ct) => db.RiesgoEmpresa.AnyAsync(r => r.Id == riesgoId, ct);

    public async Task<IReadOnlyList<EvaluacionResponseDto>> ListarAsync(Guid riesgoId, CancellationToken ct)
    {
        var filas = await db.EvaluacionRiesgo.AsNoTracking()
            .Where(e => e.RiesgoEmpresaId == riesgoId)
            .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
            .Select(e => new { Evaluacion = e, e.EvaluadoPor.Persona })
            .ToListAsync(ct);
        return filas.Select(f => EvaluacionResponseDto.De(f.Evaluacion, f.Persona.NombreCompleto)).ToList();
    }
}
