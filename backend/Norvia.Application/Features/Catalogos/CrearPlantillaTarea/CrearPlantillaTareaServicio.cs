using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.CrearPlantillaTarea.DTOs;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaTarea;

/// <summary>
/// HU-023: agrega una plantilla de tarea a un peligro activo. Desde ese momento Norvia la sugiere como control (si tiene
/// tipo) o la propone como tarea del plan (si no), en los riesgos de ese peligro.
/// </summary>
public sealed class CrearPlantillaTareaServicio(ICrearPlantillaTareaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<PlantillaTareaResponseDto>> EjecutarAsync(CrearPlantillaTareaRequestDto request, CancellationToken ct)
    {
        var peligroId = request.PeligroId!.Value;
        if (!await repositorio.PeligroActivoAsync(peligroId, ct)) return Errores.PeligroInvalido;

        var plantilla = new PlantillaTarea
        {
            PeligroId = peligroId,
            TipoControl = request.TipoControl,
            Titulo = request.Titulo.Trim(),
            Descripcion = Texto.Opcional(request.Descripcion),
            PrioridadSugerida = request.PrioridadSugerida!.Value,
            RequiereEvidencia = request.RequiereEvidencia,
            Activa = true
        };
        repositorio.Agregar(plantilla);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PlantillaTareaResponseDto.De(plantilla);
    }
}
