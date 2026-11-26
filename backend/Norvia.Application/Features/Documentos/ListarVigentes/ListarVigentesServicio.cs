using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ListarVigentes.DTOs;

namespace Norvia.Application.Features.Documentos.ListarVigentes;

/// <summary>
/// HU-074: las políticas y los procedimientos vigentes de la empresa, para cualquier persona con vínculo. Solo las
/// versiones vigentes: los borradores y las versiones reemplazadas no se muestran.
/// </summary>
public sealed class ListarVigentesServicio(IListarVigentesRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<DocumentoVigenteResumenResponseDto>>> EjecutarAsync(
        FiltrosVigentesDto filtros, CancellationToken ct)
    {
        var buscar = string.IsNullOrWhiteSpace(filtros.Buscar) ? null : filtros.Buscar.Trim();
        return Resultado<IReadOnlyList<DocumentoVigenteResumenResponseDto>>.Exito(await repositorio.ListarAsync(buscar, ct));
    }
}
