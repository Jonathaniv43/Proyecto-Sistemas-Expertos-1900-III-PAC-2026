using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.DescartarEvento.DTOs;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.DescartarEvento;

/// <summary>
/// HU-049: descarta un reporte falso, duplicado o inválido sin investigarlo ni crear medidas (RN-28). Guarda quién lo
/// descartó, cuándo y el motivo (opcional); queda fuera de las estadísticas (HU-052). Uno cerrado ya no se descarta.
/// </summary>
public sealed class DescartarEventoServicio(
    IDescartarEventoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo, IReloj reloj, ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<EventoDetalleResponseDto>> EjecutarAsync(Guid id, DescartarEventoRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.EventoNoEncontrado;
        if (await repositorio.BuscarAsync(id, ct) is not { } evento) return Errores.EventoNoEncontrado;
        if (evento.Estado is EstadoAccidente.Cerrado or EstadoAccidente.Descartado) return Errores.EventoTerminado;

        evento.Estado = EstadoAccidente.Descartado;
        evento.DescartadoPorId = vinculoId;
        evento.DescartadoEn = await reloj.AhoraAsync(ct);
        evento.MotivoDescarte = string.IsNullOrWhiteSpace(request.Motivo) ? null : request.Motivo.Trim();
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await repositorio.DetalleAsync(id, ct);
    }
}
