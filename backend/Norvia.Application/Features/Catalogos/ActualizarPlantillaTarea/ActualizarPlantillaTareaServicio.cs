using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea.DTOs;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea;

/// <summary>
/// HU-023: edita una plantilla de tarea. Lo ya aceptado o generado en las empresas no cambia: los controles (HU-039) y
/// las tareas del plan (HU-042) guardan su propia copia del texto.
/// </summary>
public sealed class ActualizarPlantillaTareaServicio(IActualizarPlantillaTareaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<PlantillaTareaResponseDto>> EjecutarAsync(Guid id, ActualizarPlantillaTareaRequestDto request, CancellationToken ct)
    {
        var plantilla = await repositorio.BuscarAsync(id, ct);
        if (plantilla is null) return Errores.PlantillaNoEncontrada;

        if (request.QuitarTipoControl == true) plantilla.TipoControl = null;
        else if (request.TipoControl is { } tipo) plantilla.TipoControl = tipo;
        if (request.Titulo is not null) plantilla.Titulo = request.Titulo.Trim();
        if (request.Descripcion is not null) plantilla.Descripcion = Texto.Opcional(request.Descripcion);
        if (request.PrioridadSugerida is { } prioridad) plantilla.PrioridadSugerida = prioridad;
        if (request.RequiereEvidencia is { } evidencia) plantilla.RequiereEvidencia = evidencia;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PlantillaTareaResponseDto.De(plantilla);
    }
}
