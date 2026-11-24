using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Plan.CambiarEstadoTarea.DTOs;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.CambiarEstadoTarea;

/// <summary>
/// HU-044: el avance de una tarea: pendiente, en progreso o completada, en cualquier orden (también reabrirla).
/// <list type="bullet">
/// <item>Solo la cambia su responsable o un Responsable SST (RN-24); a cualquier otro, 403, aunque tenga el permiso.</item>
/// <item>Para iniciarla o completarla necesita responsable y fecha límite (RN-22).</item>
/// <item>Si pide evidencia, no se completa sin ella (RN-25).</item>
/// <item>Completarla guarda cuándo; volver a abrirla lo borra. Una tarea cancelada ya no cambia.</item>
/// </list>
/// </summary>
public sealed class CambiarEstadoTareaServicio(
    ICambiarEstadoTareaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<TareaResponseDto>> EjecutarAsync(Guid tareaId, CambiarEstadoTareaRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (await repositorio.BuscarAsync(tareaId, ct) is not { } tarea) return Errores.TareaNoEncontrada;
        if (tarea.ResponsableId != vinculoId && await repositorio.RolDelVinculoAsync(vinculoId, ct) != Rol.ResponsableSstId)
            return Errores.TareaAjena;
        if (tarea.Estado == EstadoTarea.Cancelada) return Errores.TareaCancelada;

        var estado = request.Estado!.Value;
        if (estado != tarea.Estado)
        {
            if (estado != EstadoTarea.Pendiente && (tarea.ResponsableId is null || tarea.FechaLimite is null)) return Errores.TareaSinAsignar;
            if (estado == EstadoTarea.Completada && tarea.RequiereEvidencia && await repositorio.EvidenciasAsync(tarea.Id, ct) == 0)
                return Errores.FaltaEvidencia;

            tarea.Estado = estado;
            tarea.CompletadaEn = estado == EstadoTarea.Completada ? await reloj.AhoraAsync(ct) : null;
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
        }
        return await repositorio.ProyectarAsync(tarea.Id, ct);
    }
}
