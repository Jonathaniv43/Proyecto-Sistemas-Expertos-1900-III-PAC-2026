using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ListarRubros;

/// <summary>Lista todos los rubros, también los desactivados, para mantenerlos (HU-023).</summary>
public sealed class ListarRubrosServicio(IListarRubrosRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<RubroResponseDto>> EjecutarAsync(CancellationToken ct) =>
        (await repositorio.ListarAsync(ct)).Select(RubroResponseDto.De).ToList();
}
