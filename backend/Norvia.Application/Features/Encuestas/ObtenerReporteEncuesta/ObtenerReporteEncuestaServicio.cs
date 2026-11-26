using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta;

/// <summary>
/// HU-032 y HU-034: reporte anónimo de una versión publicada o reemplazada de la encuesta (RN-12, RN-72, RN-73).
/// </summary>
public sealed class ObtenerReporteEncuestaServicio(
    IObtenerReporteEncuestaRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<ReporteEncuestaResponseDto>> EjecutarAsync(
        Guid encuestaId, Guid? versionId, CancellationToken ct)
    {
        var encuesta = await repositorio.BuscarConVersionAsync(encuestaId, versionId, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        var version = encuesta.Versiones.FirstOrDefault();
        if (version is null)
            return versionId.HasValue ? Errores.VersionNoEncontrada : Errores.EncuestaSinVersionPublicada;

        if (version.Estado == EstadoVersionEncuesta.Borrador)
            return Errores.VersionSinPublicar;

        var totalParticipaciones = await repositorio.ContarParticipacionesAsync(encuestaId, ct);
        var respuestas = await repositorio.ListarRespuestasDeVersionAsync(version.Id, ct);
        var totalRespuestas = respuestas.Count;
        var periodoDesde = respuestas.Count == 0 ? (DateOnly?)null : respuestas.Min(r => r.FechaRespuesta);
        var periodoHasta = respuestas.Count == 0 ? (DateOnly?)null : respuestas.Max(r => r.FechaRespuesta);
        var ocultarDetalle = totalRespuestas < AgregadorReporteVersion.MinimoParaDetalle;

        return new ReporteEncuestaResponseDto(
            encuestaId,
            totalParticipaciones,
            totalRespuestas,
            periodoDesde,
            periodoHasta,
            ocultarDetalle,
            AgregadorReporteVersion.ArmarPreguntas(version, respuestas, ocultarDetalle),
            version.Id,
            version.NumeroVersion,
            version.Estado);
    }
}
