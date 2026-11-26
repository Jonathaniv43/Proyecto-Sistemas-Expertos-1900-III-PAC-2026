using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.ListarControlesSugeridos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarControlesSugeridos;

/// <summary>
/// HU-039: controles que Norvia sugiere a partir de las plantillas del peligro del riesgo (RN-76), del más efectivo
/// al menos efectivo según la jerarquía de controles (RN-21): eliminación, sustitución, ingeniería, administrativo y
/// equipo de protección personal. Las ya aceptadas no se repiten. Un peligro descrito por la empresa no tiene plantillas.
/// </summary>
public sealed class ListarControlesSugeridosServicio(IListarControlesSugeridosRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<ControlSugeridoResponseDto>>> EjecutarAsync(Guid riesgoId, CancellationToken ct)
    {
        var (peligroId, existe) = await repositorio.PeligroDelRiesgoAsync(riesgoId, ct);
        if (!existe) return Errores.RiesgoNoEncontrado;
        if (peligroId is not Guid id) return Resultado<IReadOnlyList<ControlSugeridoResponseDto>>.Exito([]);

        var sugerencias = await repositorio.PendientesAsync(riesgoId, id, ct);
        return Resultado<IReadOnlyList<ControlSugeridoResponseDto>>.Exito(
            sugerencias.OrderBy(s => s.Tipo).ThenBy(s => s.Titulo, StringComparer.CurrentCulture).ToList());
    }
}
