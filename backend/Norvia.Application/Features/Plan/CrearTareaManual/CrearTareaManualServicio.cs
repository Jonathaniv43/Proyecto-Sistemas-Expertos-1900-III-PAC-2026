using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Notificaciones;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Plan.CrearTareaManual.DTOs;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.CrearTareaManual;

/// <summary>
/// HU-043: una tarea que no sale de un riesgo ni de un accidente (origen manual, RN-23, D6). Queda pendiente,
/// con su responsable (vínculo activo de la empresa, RN-07) y su fecha límite, que no puede ser anterior a hoy.
/// HU-081: se avisa al responsable (RN-77).
/// </summary>
public sealed class CrearTareaManualServicio(
    ICrearTareaManualRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion,
    Notificador notificador) : IServicioCasoDeUso
{
    public async Task<Resultado<TareaResponseDto>> EjecutarAsync(CrearTareaManualRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (request.FechaLimite!.Value < Asignacion.HoyEnHonduras(await reloj.AhoraAsync(ct))) return Errores.FechaPasada;
        if (!await repositorio.EsResponsableValidoAsync(request.ResponsableId!.Value, ct)) return Errores.ResponsableInvalido;

        var tarea = new Tarea
        {
            Titulo = request.Titulo!.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            Estado = EstadoTarea.Pendiente,
            Prioridad = request.Prioridad!.Value,
            ResponsableId = request.ResponsableId,
            FechaLimite = request.FechaLimite,
            RequiereEvidencia = request.RequiereEvidencia,
            CreadaPorId = vinculoId
        };
        repositorio.Agregar(tarea);
        notificador.TareaAsignada(tarea);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await repositorio.ProyectarAsync(tarea.Id, ct);
    }
}
