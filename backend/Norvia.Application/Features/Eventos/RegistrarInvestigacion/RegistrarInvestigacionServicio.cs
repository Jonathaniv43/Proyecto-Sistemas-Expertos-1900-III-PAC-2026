using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Application.Features.Eventos.RegistrarInvestigacion.DTOs;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.RegistrarInvestigacion;

/// <summary>
/// HU-049: registra (o corrige) la investigación de un evento: causas inmediatas y de fondo, conclusiones, quién la hizo
/// (persona activa de la empresa) y cuándo (no futura ni antes del evento). Un evento abierto pasa a "en investigación".
/// No se cierra solo: cerrar es aparte y pide además medidas correctivas (RN-28).
/// </summary>
public sealed class RegistrarInvestigacionServicio(
    IRegistrarInvestigacionRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo, IReloj reloj) : IServicioCasoDeUso
{
    public async Task<Resultado<EventoDetalleResponseDto>> EjecutarAsync(Guid id, RegistrarInvestigacionRequestDto request, CancellationToken ct)
    {
        if (await repositorio.BuscarAsync(id, ct) is not { } evento) return Errores.EventoNoEncontrado;
        if (evento.Estado is EstadoAccidente.Cerrado or EstadoAccidente.Descartado) return Errores.EventoTerminado;

        var fecha = request.FechaInvestigacion!.Value;
        if (fecha > Reglas.FechaEnHonduras(await reloj.AhoraAsync(ct)) || fecha < Reglas.FechaEnHonduras(evento.FechaHoraEvento))
            return Errores.FechaInvestigacionInvalida;
        var investigador = request.InvestigadaPorId!.Value;
        if (!await repositorio.EsInvestigadorValidoAsync(investigador, ct)) return Errores.InvestigadorInvalido;

        var investigacion = evento.Investigacion;
        if (investigacion is null)
        {
            // Se agrega aparte, con la empresa del evento: la FK compuesta (EmpresaId, AccidenteId) no se reasigna.
            investigacion = new InvestigacionAccidente { EmpresaId = evento.EmpresaId, AccidenteId = evento.Id };
            repositorio.Agregar(investigacion);
        }
        investigacion.CausasInmediatas = request.CausasInmediatas!.Trim();
        investigacion.CausasBasicas = request.CausasBasicas!.Trim();
        investigacion.Conclusiones = string.IsNullOrWhiteSpace(request.Conclusiones) ? null : request.Conclusiones.Trim();
        investigacion.InvestigadaPorId = investigador;
        investigacion.FechaInvestigacion = fecha;
        evento.Estado = EstadoAccidente.EnInvestigacion;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await repositorio.DetalleAsync(evento.Id, ct);
    }
}
