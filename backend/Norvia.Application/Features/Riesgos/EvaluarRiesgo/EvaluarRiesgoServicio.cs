using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.EvaluarRiesgo.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.EvaluarRiesgo;

/// <summary>
/// HU-037: evaluación inicial de un riesgo de la matriz (art. 46.1.b).
/// <list type="bullet">
/// <item>Probabilidad y severidad del 1 al 5; el nivel (P × S) y su clasificación se calculan (RN-15, RN-16).</item>
/// <item>Cada evaluación se guarda aparte con quién y cuándo: una nueva no reemplaza la anterior (RN-20, D10).
/// La vigente es la más reciente.</item>
/// <item>Solo se evalúan riesgos confirmados: los sugeridos o descartados no están en la matriz (RN-18).</item>
/// </list>
/// HU-051: si se reevalúa a raíz de un evento, la evaluación guarda cuál (de la empresa y no descartado).
/// La evaluación residual (después de los controles) es HU-040.
/// </summary>
public sealed class EvaluarRiesgoServicio(
    IEvaluarRiesgoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<EvaluacionResponseDto>> EjecutarAsync(Guid riesgoId, EvaluarRiesgoRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.RiesgoNoEncontrado;

        var estado = await repositorio.EstadoDelRiesgoAsync(riesgoId, ct);
        if (estado is null) return Errores.RiesgoNoEncontrado;
        if (estado != EstadoRiesgo.Confirmado) return Errores.RiesgoFueraDeMatriz;
        if (request.AccidenteId is Guid accidenteId && !await repositorio.EventoEnlazableAsync(accidenteId, ct))
            return Errores.EventoInvalido;

        var evaluacion = new EvaluacionRiesgo
        {
            RiesgoEmpresaId = riesgoId,
            Tipo = TipoEvaluacion.Inicial,
            Probabilidad = request.Probabilidad!.Value,
            Severidad = request.Severidad!.Value,
            EvaluadoPorId = vinculoId,
            AccidenteId = request.AccidenteId,
            EvaluadoEn = await reloj.AhoraAsync(ct)
        };
        repositorio.Agregar(evaluacion);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return EvaluacionResponseDto.De(evaluacion, await repositorio.NombreDelVinculoAsync(vinculoId, ct));
    }
}
