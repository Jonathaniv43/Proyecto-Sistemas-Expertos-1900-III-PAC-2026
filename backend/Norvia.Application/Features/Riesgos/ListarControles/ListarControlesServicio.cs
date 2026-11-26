using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarControles;

/// <summary>HU-039: los controles de un riesgo de la empresa. De otra empresa responde 404.</summary>
public sealed class ListarControlesServicio(IListarControlesRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<ControlResponseDto>>> EjecutarAsync(Guid riesgoId, CancellationToken ct)
    {
        if (!await repositorio.ExisteRiesgoAsync(riesgoId, ct)) return Errores.RiesgoNoEncontrado;
        return Resultado<IReadOnlyList<ControlResponseDto>>.Exito(await repositorio.ListarAsync(riesgoId, ct));
    }
}
