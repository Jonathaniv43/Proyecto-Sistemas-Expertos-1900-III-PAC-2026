using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ListarPeligros;

public sealed class ListarPeligrosServicio(IListarPeligrosRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<PeligroResponseDto>> EjecutarAsync(CancellationToken ct) =>
        (await repositorio.ListarAsync(ct)).Select(PeligroResponseDto.De).ToList();
}
