using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.CerrarEvento;

/// <summary>
/// HU-049: cierra un evento. Un evento no se cierra sin causas y medidas correctivas (RN-28, art. 46.1.a): exige la
/// investigación y al menos una medida (tarea con origen en el evento, HU-050). Guarda cuándo se cerró.
/// </summary>
public sealed class CerrarEventoServicio(
    ICerrarEventoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Auditor auditor)
    : IServicioCasoDeUso
{
    public async Task<Resultado<EventoDetalleResponseDto>> EjecutarAsync(Guid id, CancellationToken ct)
    {
        if (await repositorio.BuscarAsync(id, ct) is not { } evento) return Errores.EventoNoEncontrado;
        if (evento.Estado is EstadoAccidente.Cerrado or EstadoAccidente.Descartado) return Errores.EventoTerminado;
        if (evento.Investigacion is null) return Errores.SinInvestigacion;
        if (await repositorio.MedidasAsync(id, ct) == 0) return Errores.SinMedidas;

        evento.Estado = EstadoAccidente.Cerrado;
        evento.CerradoEn = await reloj.AhoraAsync(ct);
        auditor.Registrar(AccionesAuditoria.EventoCerrado, EntidadesAuditoria.Accidente, evento.Id,
            new { evento.Tipo, evento.EsGrave });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await repositorio.DetalleAsync(id, ct);
    }
}
