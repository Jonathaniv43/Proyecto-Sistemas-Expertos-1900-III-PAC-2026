using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarRiesgos;
using Norvia.Application.Features.Riesgos.ListarRiesgos.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa, EvaluacionRiesgo, Control y Encuesta son tablas de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ListarRiesgosRepositorio(NorviaDbContext db) : IListarRiesgosRepositorio
{
    public async Task<IReadOnlyList<RiesgoResponseDto>> ListarAsync(FiltrosRiesgosDto f, CancellationToken ct)
    {
        var q = db.RiesgoEmpresa.AsNoTracking().Where(r => r.Estado == f.Estado);

        if (f.Buscar is { } texto)
        {
            var t = texto.ToLower();
            q = q.Where(r => (r.Peligro != null && r.Peligro.Nombre.ToLower().Contains(t))
                             || (r.DescripcionPeligro != null && r.DescripcionPeligro.ToLower().Contains(t))
                             || (r.Actividad != null && r.Actividad.ToLower().Contains(t)));
        }
        if (f.ClasificacionPeligro is { } clasificacion)
            q = q.Where(r => r.Peligro != null && r.Peligro.Clasificacion == clasificacion);
        if (f.SinEvaluar == true)
            q = q.Where(r => !r.Evaluaciones.Any(e => e.Tipo == TipoEvaluacion.Inicial));

        // Nivel y celda: los de la última evaluación inicial. Sin evaluación, el nivel queda en 0.
        if (f.Nivel is { } nivel)
        {
            var (min, max) = nivel switch
            {
                ClasificacionRiesgo.Bajo => (1, 4),
                ClasificacionRiesgo.Medio => (5, 9),
                ClasificacionRiesgo.Alto => (10, 14),
                _ => (15, 25)
            };
            q = q.Where(r => r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                .Select(e => e.Probabilidad * e.Severidad).FirstOrDefault() >= min
                && r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                .Select(e => e.Probabilidad * e.Severidad).FirstOrDefault() <= max);
        }
        // RN-19: altos y críticos (nivel 10 o más) sin ningún control.
        if (f.SinControl == true)
            q = q.Where(r => !r.Controles.Any()
                && r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => e.Probabilidad * e.Severidad).FirstOrDefault() >= 10);
        if (f.Probabilidad is { } p && f.Severidad is { } s)
            q = q.Where(r => r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => e.Probabilidad).FirstOrDefault() == p
                && r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => e.Severidad).FirstOrDefault() == s);

        q = q.OrderByDescending(r => r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                .Select(e => e.Probabilidad * e.Severidad).FirstOrDefault())
            .ThenByDescending(r => r.CreadoEn);

        return await ProyeccionRiesgo.ListarAsync(db, q, ct);
    }
}
