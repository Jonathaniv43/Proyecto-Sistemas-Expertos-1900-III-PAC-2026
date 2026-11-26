using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Vinculos.DTOs;

namespace Norvia.Application.Features.Vinculos.ObtenerVinculo;

public sealed class ObtenerVinculoServicio(IObtenerVinculoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<VinculoResponseDto>> EjecutarAsync(Guid id, CancellationToken ct) =>
        await repositorio.BuscarAsync(id, ct) is { } vinculo
            ? VinculoResponseDto.De(vinculo)
            : Errores.VinculoNoEncontrado;
}
