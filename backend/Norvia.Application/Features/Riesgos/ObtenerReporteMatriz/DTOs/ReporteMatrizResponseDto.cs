using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ObtenerMatriz.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ObtenerReporteMatriz.DTOs;

/// <summary>Datos empresariales de cabecera incluidos en el reporte imprimible/PDF de la matriz (HU-075).</summary>
public sealed record EmpresaReporteMatrizDto(
    string RazonSocial,
    string Rtn,
    string? RubroNombre,
    string? ActividadEconomica,
    string? Direccion,
    int? TrabajadoresPermanentes,
    DateOnly? FechaInicioOperaciones);

/// <summary>Riesgo confirmado con su peligro, actividad, última evaluación inicial y residual, y controles ordenados por RN-21.</summary>
public sealed record RiesgoReporteMatrizDto(
    Guid Id,
    Guid? PeligroId,
    string? PeligroNombre,
    ClasificacionPeligro? PeligroClasificacion,
    string? DescripcionPeligro,
    string? Actividad,
    Guid? EncuestaId,
    string? EncuestaTitulo,
    Guid? AccidenteId,
    DateTime CreadoEn,
    EvaluacionResponseDto? Evaluacion,
    EvaluacionResponseDto? EvaluacionResidual,
    IReadOnlyList<ControlResponseDto> Controles);

/// <summary>Reporte oficial completo de la matriz de riesgos generado al vuelo sin persistirse (HU-075, art. 47.b, RN-52).</summary>
public sealed record ReporteMatrizResponseDto(
    DateTime GeneradoEn,
    EmpresaReporteMatrizDto Empresa,
    MatrizRiesgosResponseDto Matriz,
    IReadOnlyList<RiesgoReporteMatrizDto> Riesgos);
