using Microsoft.EntityFrameworkCore;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ObtenerReporteMatriz;
using Norvia.Application.Features.Riesgos.ObtenerReporteMatriz.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa, EvaluacionRiesgo, Control y Encuesta son tablas de empresa: el filtro global limita a la empresa de la sesión.
// Empresa es tabla raíz: se filtra explícitamente con ITenantActual.EmpresaId (RN-05, RN-06).
public sealed class ObtenerReporteMatrizRepositorio(
    NorviaDbContext db,
    ITenantActual tenant) : IObtenerReporteMatrizRepositorio
{
    private sealed record UltimaEvaluacion(
        Guid Id,
        TipoEvaluacion Tipo,
        short Probabilidad,
        short Severidad,
        Guid EvaluadoPorId,
        Persona Persona,
        DateTime EvaluadoEn,
        Guid? AccidenteId);

    public async Task<(EmpresaReporteMatrizDto Empresa, IReadOnlyList<RiesgoReporteMatrizDto> Riesgos)> ObtenerDatosAsync(
        CancellationToken ct)
    {
        var empresaId = tenant.EmpresaId
            ?? throw new InvalidOperationException("Se requiere una empresa activa en la sesión.");

        var empresa = await db.Empresa
            .AsNoTracking()
            .Where(e => e.Id == empresaId)
            .Select(e => new EmpresaReporteMatrizDto(
                e.RazonSocial,
                e.RTN,
                e.Rubro != null ? e.Rubro.Nombre : null,
                e.ActividadEconomica,
                e.Direccion,
                e.TrabajadoresPermanentes,
                e.FechaInicioOperaciones))
            .SingleAsync(ct);

        var filas = await db.RiesgoEmpresa
            .AsNoTracking()
            .Where(r => r.Estado == EstadoRiesgo.Confirmado)
            .OrderByDescending(r => r.Evaluaciones
                .Where(e => e.Tipo == TipoEvaluacion.Inicial)
                .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                .Select(e => e.Probabilidad * e.Severidad)
                .FirstOrDefault())
            .ThenByDescending(r => r.CreadoEn)
            .Select(r => new
            {
                r.Id,
                r.PeligroId,
                PeligroNombre = r.Peligro != null ? r.Peligro.Nombre : null,
                PeligroClasificacion = r.Peligro != null ? r.Peligro.Clasificacion : (ClasificacionPeligro?)null,
                r.DescripcionPeligro,
                r.Actividad,
                r.EncuestaId,
                EncuestaTitulo = db.Encuesta.Where(e => e.Id == r.EncuestaId).Select(e => e.Titulo).FirstOrDefault(),
                r.AccidenteId,
                r.CreadoEn,
                Evaluacion = r.Evaluaciones
                    .Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => new UltimaEvaluacion(
                        e.Id, e.Tipo, e.Probabilidad, e.Severidad, e.EvaluadoPorId, e.EvaluadoPor.Persona, e.EvaluadoEn, e.AccidenteId))
                    .FirstOrDefault(),
                Residual = r.Evaluaciones
                    .Where(e => e.Tipo == TipoEvaluacion.Residual)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => new UltimaEvaluacion(
                        e.Id, e.Tipo, e.Probabilidad, e.Severidad, e.EvaluadoPorId, e.EvaluadoPor.Persona, e.EvaluadoEn, e.AccidenteId))
                    .FirstOrDefault(),
                Controles = r.Controles.ToList()
            })
            .ToListAsync(ct);

        var riesgos = filas
            .Select(f => new RiesgoReporteMatrizDto(
                f.Id,
                f.PeligroId,
                f.PeligroNombre,
                f.PeligroClasificacion,
                f.DescripcionPeligro,
                f.Actividad,
                f.EncuestaId,
                f.EncuestaTitulo,
                f.AccidenteId,
                f.CreadoEn,
                MapearEvaluacion(f.Evaluacion),
                MapearEvaluacion(f.Residual),
                // Jerarquía de controles (RN-21) en memoria porque TipoControl se almacena como snake_case en PostgreSQL.
                f.Controles
                    .OrderBy(c => c.Tipo)
                    .ThenBy(c => c.CreadoEn)
                    .ThenBy(c => c.Id)
                    .Select(ControlResponseDto.De)
                    .ToList()))
            .ToList();

        return (empresa, riesgos);
    }

    private static EvaluacionResponseDto? MapearEvaluacion(UltimaEvaluacion? e) => e is null
        ? null
        : new EvaluacionResponseDto(
            e.Id,
            e.Tipo,
            e.Probabilidad,
            e.Severidad,
            e.Probabilidad * e.Severidad,
            EvaluacionRiesgo.Clasificar(e.Probabilidad * e.Severidad),
            e.EvaluadoPorId,
            e.Persona.NombreCompleto,
            e.EvaluadoEn,
            e.AccidenteId);
}
