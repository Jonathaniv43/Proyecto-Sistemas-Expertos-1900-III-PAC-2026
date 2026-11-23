using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.ReportarEvento.DTOs;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ReportarEvento;

/// <summary>
/// HU-047: registra un evento de forma anónima dentro de la empresa de la sesión (D11, RN-26, RN-27).
/// <para>Pendiente HU-100: adjuntar evidencias en endpoint posterior <c>POST /api/eventos/{id}/evidencias</c>.</para>
/// </summary>
public sealed class ReportarEventoServicio(
    IReportarEventoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj) : IServicioCasoDeUso
{
    private static readonly TimeSpan MargenDesfaseReloj = TimeSpan.FromMinutes(5);

    public async Task<Resultado<ReportarEventoResponseDto>> EjecutarAsync(
        ReportarEventoRequestDto request, CancellationToken ct)
    {
        var ahora = await reloj.AhoraAsync(ct);
        var fechaEventoUtc = NormalizarUtc(request.FechaHoraEvento!.Value);
        if (fechaEventoUtc > ahora.Add(MargenDesfaseReloj))
            return Errores.FechaEventoFutura;

        // Anonimato (D11): el vínculo de la sesión no se guarda ni se expone;
        // AsignarEmpresaInterceptor fija EmpresaId desde el JWT al guardar.
        var evento = new Accidente
        {
            Tipo = request.Tipo!.Value,
            EstadoEnfermedad = request.EstadoEnfermedad,
            EsGrave = request.EsGrave,
            FechaHoraEvento = fechaEventoUtc,
            Lugar = string.IsNullOrWhiteSpace(request.Lugar) ? null : request.Lugar.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Estado = EstadoAccidente.Abierto
        };

        repositorio.Agregar(evento);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return new ReportarEventoResponseDto(
            evento.Id,
            evento.Tipo,
            evento.Estado,
            evento.EstadoEnfermedad,
            evento.EsGrave,
            evento.FechaHoraEvento,
            evento.RegistradoEn);
    }

    private static DateTime NormalizarUtc(DateTime fecha) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
    };
}
