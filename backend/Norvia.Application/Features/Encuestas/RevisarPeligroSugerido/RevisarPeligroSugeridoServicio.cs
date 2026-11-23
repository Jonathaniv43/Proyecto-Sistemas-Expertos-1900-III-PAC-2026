using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.RevisarPeligroSugerido.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.RevisarPeligroSugerido;

/// <summary>
/// HU-035: el responsable confirma o descarta un peligro sugerido por la encuesta (RN-18).
/// <list type="bullet">
/// <item>La sugerencia es el <c>RiesgoEmpresa</c> en estado <c>sugerido</c> que registró HU-033; al confirmarla queda
/// como riesgo de la empresa (sin áreas, D4) y puede indicar la actividad donde se presenta.</item>
/// <item>Solo se revisa una vez: una sugerencia confirmada o descartada no cambia por este medio.</item>
/// <item>Solo los riesgos confirmados forman la matriz; los sugeridos y descartados no aparecen en ella.</item>
/// </list>
/// </summary>
public sealed class RevisarPeligroSugeridoServicio(
    IRevisarPeligroSugeridoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<PeligroSugeridoRevisadoResponseDto>> EjecutarAsync(
        Guid encuestaId, Guid riesgoId, RevisarPeligroSugeridoRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SugerenciaNoEncontrada;

        var riesgo = await repositorio.BuscarAsync(encuestaId, riesgoId, ct);
        if (riesgo is null) return Errores.SugerenciaNoEncontrada;
        if (riesgo.Estado != EstadoRiesgo.Sugerido) return Errores.SugerenciaYaRevisada;

        riesgo.Estado = request.Estado!.Value;
        if (riesgo.Estado == EstadoRiesgo.Confirmado)
            riesgo.Actividad = string.IsNullOrWhiteSpace(request.Actividad) ? null : request.Actividad.Trim();
        riesgo.RevisadoPorId = vinculoId;
        riesgo.RevisadoEn = await reloj.AhoraAsync(ct);

        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return PeligroSugeridoRevisadoResponseDto.De(riesgo);
    }
}
