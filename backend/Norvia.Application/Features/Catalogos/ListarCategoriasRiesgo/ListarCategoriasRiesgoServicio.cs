using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ListarCategoriasRiesgo;

/// <summary>Lista las categorías de riesgo, opcionalmente solo las de un rubro.</summary>
public sealed class ListarCategoriasRiesgoServicio(IListarCategoriasRiesgoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<CategoriaRiesgoResponseDto>> EjecutarAsync(short? rubroId, CancellationToken ct) =>
        (await repositorio.ListarAsync(rubroId, ct)).Select(CategoriaRiesgoResponseDto.De).ToList();
}
