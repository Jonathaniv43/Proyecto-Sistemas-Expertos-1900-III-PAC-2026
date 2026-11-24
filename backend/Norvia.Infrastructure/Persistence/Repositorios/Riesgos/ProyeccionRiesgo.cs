using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

/// <summary>Compartido por los repositorios del módulo Riesgos que devuelven <see cref="RiesgoResponseDto"/>.</summary>
internal static class ProyeccionRiesgo
{
    private sealed record UltimaEvaluacion(
        Guid Id, TipoEvaluacion Tipo, short Probabilidad, short Severidad, Guid EvaluadoPorId, Persona Persona, DateTime EvaluadoEn);

    private sealed record Fila(
        Guid Id, EstadoRiesgo Estado, Guid? PeligroId, string? PeligroNombre, ClasificacionPeligro? PeligroClasificacion,
        string? DescripcionPeligro, string? Actividad,
        Guid? EncuestaId, string? EncuestaTitulo, DateTime CreadoEn, UltimaEvaluacion? Evaluacion);

    /// <summary>
    /// Nombre del peligro (catálogo global), título de la encuesta que lo sugirió y la última evaluación inicial
    /// con su evaluador (todo de la misma empresa). El nivel, la clasificación y el nombre completo se calculan en memoria.
    /// </summary>
    internal static async Task<List<RiesgoResponseDto>> ListarAsync(NorviaDbContext db, IQueryable<RiesgoEmpresa> riesgos, CancellationToken ct)
    {
        var filas = await riesgos
            .Select(r => new Fila(
                r.Id, r.Estado, r.PeligroId, r.Peligro != null ? r.Peligro.Nombre : null,
                r.Peligro != null ? r.Peligro.Clasificacion : (ClasificacionPeligro?)null, r.DescripcionPeligro, r.Actividad,
                r.EncuestaId, db.Encuesta.Where(e => e.Id == r.EncuestaId).Select(e => e.Titulo).FirstOrDefault(), r.CreadoEn,
                r.Evaluaciones
                    .Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => new UltimaEvaluacion(e.Id, e.Tipo, e.Probabilidad, e.Severidad, e.EvaluadoPorId, e.EvaluadoPor.Persona, e.EvaluadoEn))
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return filas.Select(f => new RiesgoResponseDto(
                f.Id, f.Estado, f.PeligroId, f.PeligroNombre, f.PeligroClasificacion, f.DescripcionPeligro, f.Actividad, f.EncuestaId, f.EncuestaTitulo, f.CreadoEn,
                f.Evaluacion is { } e
                    ? new EvaluacionResponseDto(e.Id, e.Tipo, e.Probabilidad, e.Severidad, e.Probabilidad * e.Severidad,
                        EvaluacionRiesgo.Clasificar(e.Probabilidad * e.Severidad), e.EvaluadoPorId, e.Persona.NombreCompleto, e.EvaluadoEn)
                    : null))
            .ToList();
    }
}
