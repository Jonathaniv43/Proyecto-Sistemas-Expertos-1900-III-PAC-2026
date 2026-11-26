using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.CrearControl.DTOs;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.CrearControl;

/// <summary>
/// HU-039: agrega un control a un riesgo de la matriz (art. 46.1.e).
/// <list type="bullet">
/// <item>Cada control indica su tipo en la jerarquía y su descripción (RN-21).</item>
/// <item>Puede ser una sugerencia aceptada (tal cual o editada) o uno propio. Una sugerencia solo se guarda al
/// aceptarla (RN-76), una sola vez por riesgo, y tiene que ser del peligro del riesgo.</item>
/// <item>Solo los riesgos confirmados tienen controles: los sugeridos o descartados no están en la matriz (RN-18).</item>
/// <item>Queda <c>propuesto</c>; se implementa con el plan de trabajo (M5).</item>
/// </list>
/// </summary>
public sealed class CrearControlServicio(
    ICrearControlRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<Resultado<ControlResponseDto>> EjecutarAsync(Guid riesgoId, CrearControlRequestDto request, CancellationToken ct)
    {
        if (await repositorio.RiesgoAsync(riesgoId, ct) is not { } riesgo) return Errores.RiesgoNoEncontrado;
        if (riesgo.Estado != EstadoRiesgo.Confirmado) return Errores.RiesgoFueraDeMatriz;

        if (request.PlantillaId is Guid plantillaId)
        {
            if (riesgo.PeligroId is not Guid peligroId || !await repositorio.EsSugerenciaDelPeligroAsync(plantillaId, peligroId, ct))
                return Errores.SugerenciaInvalida;
            if (await repositorio.SugerenciaYaAceptadaAsync(riesgoId, plantillaId, ct))
                return Errores.SugerenciaYaAceptada;
        }

        var control = new Control
        {
            RiesgoEmpresaId = riesgoId,
            PlantillaTareaId = request.PlantillaId,
            Tipo = request.Tipo!.Value,
            Descripcion = request.Descripcion!.Trim(),
            Estado = EstadoControl.Propuesto
        };
        repositorio.Agregar(control);
        auditor.Registrar(AccionesAuditoria.ControlCreado, EntidadesAuditoria.Control, control.Id,
            new { riesgoId, control.Tipo, control.Descripcion, control.PlantillaTareaId });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return ControlResponseDto.De(control);
    }
}
