using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual;

/// <summary>
/// HU-040: evaluación residual de un riesgo, con sus controles aplicados, para saber si el riesgo bajó.
/// <list type="bullet">
/// <item>Se guarda aparte de la inicial (tipo <c>residual</c>, D10); una nueva no reemplaza la anterior (RN-20).</item>
/// <item>Mismo cálculo que la inicial: P × S del 1 al 5 y su clasificación (RN-15, RN-16).</item>
/// <item>Solo para riesgos de la matriz (RN-18) que ya tienen evaluación inicial (no hay con qué comparar) y al menos
/// un control (sin controles no hay "riesgo con los controles aplicados").</item>
/// </list>
/// </summary>
public sealed class EvaluarRiesgoResidualServicio(
    IEvaluarRiesgoResidualRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<Resultado<EvaluacionResponseDto>> EjecutarAsync(Guid riesgoId, EvaluarRiesgoResidualRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.RiesgoNoEncontrado;

        if (await repositorio.RiesgoAsync(riesgoId, ct) is not { } riesgo) return Errores.RiesgoNoEncontrado;
        if (riesgo.Estado != EstadoRiesgo.Confirmado) return Errores.RiesgoFueraDeMatriz;
        if (!riesgo.TieneInicial) return Errores.SinEvaluacionInicial;
        if (!riesgo.TieneControles) return Errores.SinControles;

        var evaluacion = new EvaluacionRiesgo
        {
            RiesgoEmpresaId = riesgoId,
            Tipo = TipoEvaluacion.Residual,
            Probabilidad = request.Probabilidad!.Value,
            Severidad = request.Severidad!.Value,
            EvaluadoPorId = vinculoId,
            EvaluadoEn = await reloj.AhoraAsync(ct)
        };
        repositorio.Agregar(evaluacion);
        auditor.Registrar(AccionesAuditoria.RiesgoEvaluadoResidual, EntidadesAuditoria.EvaluacionRiesgo, evaluacion.Id, new
        {
            riesgoId,
            evaluacion.Tipo,
            evaluacion.Probabilidad,
            evaluacion.Severidad,
            nivel = evaluacion.Probabilidad * evaluacion.Severidad
        });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return EvaluacionResponseDto.De(evaluacion, await repositorio.NombreDelVinculoAsync(vinculoId, ct));
    }
}
