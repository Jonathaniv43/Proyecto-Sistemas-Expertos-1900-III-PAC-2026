using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.CrearRiesgo.DTOs;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.CrearRiesgo;

/// <summary>
/// HU-036: el responsable agrega a la matriz un riesgo que la encuesta no detectó (D9).
/// <list type="bullet">
/// <item>Un peligro del catálogo (activo) o uno descrito, más la actividad donde se presenta.</item>
/// <item>Entra directo como <c>confirmado</c>: lo decide quien lo agrega, así que queda como su revisor.</item>
/// <item>No se repite el mismo peligro con la misma actividad en la matriz.</item>
/// <item>Es de la empresa de la sesión (D4): EmpresaId lo asigna el interceptor, nunca el cliente.</item>
/// </list>
/// </summary>
public sealed class CrearRiesgoServicio(
    ICrearRiesgoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<RiesgoResponseDto>> EjecutarAsync(CrearRiesgoRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.RiesgoNoEncontrado;

        (string Nombre, ClasificacionPeligro Clasificacion)? peligro = null;
        if (request.PeligroId is Guid peligroId)
        {
            peligro = await repositorio.PeligroActivoAsync(peligroId, ct);
            if (peligro is null) return Errores.PeligroInvalido;
        }
        var descripcion = string.IsNullOrWhiteSpace(request.DescripcionPeligro) ? null : request.DescripcionPeligro.Trim();
        var actividad = request.Actividad.Trim();

        if (await repositorio.ExisteEnMatrizAsync(request.PeligroId, descripcion, actividad, ct))
            return Errores.RiesgoDuplicado;

        var riesgo = new RiesgoEmpresa
        {
            PeligroId = request.PeligroId,
            DescripcionPeligro = request.PeligroId is null ? descripcion : null,
            Actividad = actividad,
            Estado = EstadoRiesgo.Confirmado,
            RevisadoPorId = vinculoId,
            RevisadoEn = await reloj.AhoraAsync(ct)
        };
        repositorio.Agregar(riesgo);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return new RiesgoResponseDto(riesgo.Id, riesgo.Estado, riesgo.PeligroId, peligro?.Nombre, peligro?.Clasificacion, riesgo.DescripcionPeligro,
            riesgo.Actividad, null, null, riesgo.CreadoEn, null);
    }
}
