using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.ListarRiesgosDelEvento;
using Norvia.Application.Features.Eventos.ListarRiesgosDelEvento.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

// Accidente, RiesgoEmpresa y EvaluacionRiesgo son tablas de empresa: el filtro global limita a la sesión.
public sealed class ListarRiesgosDelEventoRepositorio(NorviaDbContext db) : IListarRiesgosDelEventoRepositorio
{
    public Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct) => db.Accidente.AnyAsync(a => a.Id == eventoId, ct);

    public async Task<List<RiesgoDelEventoResponseDto>> ListarAsync(Guid eventoId, CancellationToken ct)
    {
        var filas = await db.RiesgoEmpresa.AsNoTracking()
            .Where(r => r.AccidenteId == eventoId || r.Evaluaciones.Any(e => e.AccidenteId == eventoId))
            .Select(r => new
            {
                r.Id,
                Nombre = r.Peligro != null ? r.Peligro.Nombre : r.DescripcionPeligro!,
                r.Actividad,
                Revelado = r.AccidenteId == eventoId,
                // Las iniciales, de la más antigua a la más reciente.
                Iniciales = r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderBy(e => e.EvaluadoEn).ThenBy(e => e.Id)
                    .Select(e => new Inicial(e.Probabilidad * e.Severidad, e.EvaluadoEn, e.AccidenteId))
                    .ToList()
            })
            .ToListAsync(ct);

        return filas
            .Select(f =>
            {
                if (f.Revelado)
                {
                    var actual = f.Iniciales.LastOrDefault();
                    return Dto(f.Id, f.Nombre, f.Actividad, RelacionRiesgoEvento.Revelado, null, actual);
                }
                var i = f.Iniciales.FindLastIndex(e => e.AccidenteId == eventoId);
                return Dto(f.Id, f.Nombre, f.Actividad, RelacionRiesgoEvento.Reevaluado, i > 0 ? f.Iniciales[i - 1] : null, f.Iniciales[i]);
            })
            .OrderBy(r => r.Relacion).ThenByDescending(r => r.EvaluadoEn).ThenBy(r => r.Nombre, StringComparer.CurrentCulture)
            .ToList();
    }

    private sealed record Inicial(int Nivel, DateTime EvaluadoEn, Guid? AccidenteId);

    private static RiesgoDelEventoResponseDto Dto(
        Guid id, string nombre, string? actividad, RelacionRiesgoEvento relacion, Inicial? anterior, Inicial? evaluacion) =>
        new(id, nombre, actividad, relacion, anterior?.Nivel, anterior is null ? null : EvaluacionRiesgo.Clasificar(anterior.Nivel),
            evaluacion?.Nivel, evaluacion is null ? null : EvaluacionRiesgo.Clasificar(evaluacion.Nivel), evaluacion?.EvaluadoEn);
}
