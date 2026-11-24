using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ObtenerMatriz.DTOs;

/// <summary>Una celda del mapa con riesgos: cuántos tienen esa probabilidad y severidad en su última evaluación.</summary>
public sealed record CeldaMatrizDto(short Probabilidad, short Severidad, int Nivel, ClasificacionRiesgo Clasificacion, int Cantidad);

/// <summary>
/// Mapa de la matriz (HU-038): los riesgos confirmados ubicados por su última evaluación inicial.
/// <paramref name="Celdas"/> solo trae las celdas con riesgos; <paramref name="SinEvaluar"/> cuenta los que no se ubican.
/// <paramref name="AltosSinControl"/> cuenta los altos o críticos que todavía no tienen controles (RN-19, HU-039).
/// </summary>
public sealed record MatrizRiesgosResponseDto(int Total, int SinEvaluar, int AltosSinControl, IReadOnlyList<CeldaMatrizDto> Celdas);
