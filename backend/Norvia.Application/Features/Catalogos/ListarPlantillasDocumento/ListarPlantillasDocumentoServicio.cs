using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ListarPlantillasDocumento;

public sealed class ListarPlantillasDocumentoServicio(IListarPlantillasDocumentoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<PlantillaDocumentoResponseDto>> EjecutarAsync(CancellationToken ct) =>
        (await repositorio.ListarAsync(ct)).Select(PlantillaDocumentoResponseDto.De).ToList();
}
