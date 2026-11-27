using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Riesgos.ObtenerMatriz.DTOs;

namespace Norvia.Application.Features.Riesgos.ObtenerMatriz;

/// <summary>HU-038: cuántos riesgos de la matriz hay en cada celda de probabilidad × severidad (art. 47.b).</summary>
public sealed class ObtenerMatrizServicio(IObtenerMatrizRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<MatrizRiesgosResponseDto> EjecutarAsync(CancellationToken ct)
    {
        var ubicaciones = await repositorio.UbicacionesAsync(ct);
        return ArmadorMatriz.Construir(ubicaciones);
    }
}
