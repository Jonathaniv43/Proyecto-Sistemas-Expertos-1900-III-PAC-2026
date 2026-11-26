using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea.DTOs;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea;

/// <summary>
/// HU-023: activa o desactiva una plantilla de tarea. Desactivada, Norvia deja de sugerirla y de proponerla en el plan;
/// lo ya aceptado o generado en las empresas se conserva.
/// </summary>
public sealed class CambiarEstadoPlantillaTareaServicio(ICambiarEstadoPlantillaTareaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<PlantillaTareaResponseDto>> EjecutarAsync(Guid id, CambiarEstadoPlantillaTareaRequestDto request, CancellationToken ct)
    {
        var plantilla = await repositorio.BuscarAsync(id, ct);
        if (plantilla is null) return Errores.PlantillaNoEncontrada;

        plantilla.Activa = request.Activo!.Value; // el validador exige el valor
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PlantillaTareaResponseDto.De(plantilla);
    }
}
