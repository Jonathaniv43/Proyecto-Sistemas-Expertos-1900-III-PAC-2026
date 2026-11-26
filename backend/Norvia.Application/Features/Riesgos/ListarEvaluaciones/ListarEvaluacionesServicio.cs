using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarEvaluaciones;

/// <summary>
/// HU-041: el historial de evaluaciones de un riesgo, para demostrar su evolución (art. 47.b). Como ninguna
/// evaluación se reemplaza (RN-20, D10), están todas: iniciales y residuales, con fecha y quién evaluó.
/// De otra empresa responde 404.
/// </summary>
public sealed class ListarEvaluacionesServicio(IListarEvaluacionesRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<EvaluacionResponseDto>>> EjecutarAsync(Guid riesgoId, CancellationToken ct)
    {
        if (!await repositorio.ExisteRiesgoAsync(riesgoId, ct)) return Errores.RiesgoNoEncontrado;
        return Resultado<IReadOnlyList<EvaluacionResponseDto>>.Exito(await repositorio.ListarAsync(riesgoId, ct));
    }
}
