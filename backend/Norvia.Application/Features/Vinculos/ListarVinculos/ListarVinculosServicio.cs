using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Vinculos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.ListarVinculos;

/// <summary>Lista las personas con acceso a la empresa de la sesión, con filtros opcionales.</summary>
public sealed class ListarVinculosServicio(IListarVinculosRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<VinculoResponseDto>> EjecutarAsync(
        EstadoVinculo? estado, string? rolCodigo, CancellationToken ct) =>
        (await repositorio.ListarAsync(estado, string.IsNullOrWhiteSpace(rolCodigo) ? null : rolCodigo.Trim(), ct))
            .Select(VinculoResponseDto.De).ToList();
}
