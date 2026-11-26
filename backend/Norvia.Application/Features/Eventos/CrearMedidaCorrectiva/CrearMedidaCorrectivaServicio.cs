using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Notificaciones;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.CrearMedidaCorrectiva.DTOs;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.CrearMedidaCorrectiva;

/// <summary>
/// HU-050: convierte una medida correctiva en una tarea del plan de trabajo con origen "accidente" (RN-23, D6,
/// art. 46.1.a). Queda pendiente, con responsable (persona activa de la empresa) y fecha límite no anterior a hoy.
/// Solo mientras el evento siga abierto o en investigación: cerrado o descartado ya no recibe medidas.
/// HU-081: se avisa al responsable (RN-77).
/// </summary>
public sealed class CrearMedidaCorrectivaServicio(
    ICrearMedidaCorrectivaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion,
    Notificador notificador,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<Resultado<MedidaCorrectivaResponseDto>> EjecutarAsync(
        Guid eventoId, CrearMedidaCorrectivaRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (await repositorio.BuscarAsync(eventoId, ct) is not { } evento) return Errores.EventoNoEncontrado;
        if (evento.Estado is EstadoAccidente.Cerrado or EstadoAccidente.Descartado) return Errores.EventoTerminado;
        if (request.FechaLimite!.Value < Reglas.FechaEnHonduras(await reloj.AhoraAsync(ct))) return Errores.FechaLimitePasada;
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
            AccidenteId = evento.Id,
            CreadaPorId = vinculoId
        };
        repositorio.Agregar(tarea);
        notificador.TareaAsignada(tarea);
        auditor.Registrar(AccionesAuditoria.MedidaCorrectivaCreada, EntidadesAuditoria.Accidente, evento.Id,
            new { tareaId = tarea.Id, tarea.Titulo, tarea.Prioridad, tarea.ResponsableId });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await repositorio.ProyectarAsync(tarea.Id, ct);
    }
}
