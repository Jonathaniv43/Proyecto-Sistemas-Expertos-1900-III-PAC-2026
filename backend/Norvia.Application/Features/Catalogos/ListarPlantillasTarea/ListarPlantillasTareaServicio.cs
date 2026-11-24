using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ListarPlantillasTarea;

/// <summary>
/// HU-023: plantillas de tarea, opcionalmente solo las de un peligro. Por peligro: primero los controles, del más
/// efectivo al menos efectivo (RN-21), y después las tareas modelo; dentro de cada grupo, por título.
/// </summary>
public sealed class ListarPlantillasTareaServicio(IListarPlantillasTareaRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<PlantillaTareaResponseDto>> EjecutarAsync(Guid? peligroId, CancellationToken ct) =>
        (await repositorio.ListarAsync(peligroId, ct))
            .OrderBy(p => p.PeligroId)
            .ThenBy(p => p.TipoControl is null).ThenBy(p => p.TipoControl)
            .ThenBy(p => p.Titulo, StringComparer.CurrentCulture)
            .Select(PlantillaTareaResponseDto.De)
            .ToList();
}
