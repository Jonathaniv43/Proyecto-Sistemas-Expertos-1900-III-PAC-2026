using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta;

/// <summary>
/// HU-032: reporte anónimo de la versión publicada vigente (RN-72).
/// El detalle se muestra con cualquier cantidad de respuestas.
/// </summary>
public sealed class ObtenerReporteEncuestaServicio(
    IObtenerReporteEncuestaRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<ReporteEncuestaResponseDto>> EjecutarAsync(
        Guid encuestaId, CancellationToken ct)
    {
        var encuesta = await repositorio.BuscarConVersionPublicadaAsync(encuestaId, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        var version = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Publicada);
        if (version is null)
            return Errores.EncuestaSinVersionPublicada;

        var totalParticipaciones = await repositorio.ContarParticipacionesAsync(encuestaId, ct);
        var respuestas = await repositorio.ListarRespuestasDeVersionAsync(version.Id, ct);
        var totalRespuestas = respuestas.Count;
        var periodoDesde = respuestas.Count == 0 ? (DateOnly?)null : respuestas.Min(r => r.FechaRespuesta);
        var periodoHasta = respuestas.Count == 0 ? (DateOnly?)null : respuestas.Max(r => r.FechaRespuesta);

        var preguntas = version.Preguntas
            .OrderBy(p => p.Orden)
            .Select(p => ArmarPregunta(p, respuestas))
            .ToList();

        return new ReporteEncuestaResponseDto(
            encuestaId,
            totalParticipaciones,
            totalRespuestas,
            periodoDesde,
            periodoHasta,
            DetalleOculto: false,
            preguntas);
    }

    private static PreguntaReporteResponseDto ArmarPregunta(
        Domain.Entities.Encuestas.EncuestaPregunta pregunta,
        List<Domain.Entities.Encuestas.RespuestaEncuesta> respuestas)
    {
        var codigo = pregunta.TipoRespuestaPregunta?.Codigo ?? string.Empty;
        var detalles = respuestas
            .SelectMany(r => r.Detalles)
            .Where(d => d.EncuestaPreguntaId == pregunta.Id)
            .ToList();

        if (TipoRespuestaPreguntaCatalogo.EsCodigoRespuestaBreve(codigo))
        {
            var textos = detalles
                .Select(d => d.TextoLibre)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!)
                .ToList();
            return new PreguntaReporteResponseDto(
                pregunta.Id, pregunta.Texto, codigo, null, textos);
        }

        if (TipoRespuestaPreguntaCatalogo.CodigoRequiereOpciones(codigo))
        {
            var conteos = new Dictionary<Guid, int>();
            foreach (var opcion in pregunta.Opciones)
                conteos[opcion.Id] = 0;

            if (TipoRespuestaPreguntaCatalogo.NormalizarCodigo(codigo)
                == TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica)
            {
                foreach (var d in detalles.Where(d => d.EncuestaOpcionId is not null))
                {
                    var id = d.EncuestaOpcionId!.Value;
                    if (conteos.ContainsKey(id))
                        conteos[id]++;
                }
            }
            else
            {
                foreach (var d in detalles)
                {
                    foreach (var o in d.Opciones)
                    {
                        if (conteos.ContainsKey(o.EncuestaOpcionId))
                            conteos[o.EncuestaOpcionId]++;
                    }
                }
            }

            var basePorcentaje = Math.Max(detalles.Count, 1);
            var distribucion = pregunta.Opciones
                .OrderBy(o => o.Orden)
                .Select(o =>
                {
                    var cantidad = conteos[o.Id];
                    var porcentaje = Math.Round(100.0 * cantidad / basePorcentaje, 1);
                    return new DistribucionOpcionResponseDto(o.Id, o.Texto, cantidad, porcentaje);
                })
                .ToList();

            return new PreguntaReporteResponseDto(
                pregunta.Id, pregunta.Texto, codigo, distribucion, null);
        }

        return new PreguntaReporteResponseDto(
            pregunta.Id, pregunta.Texto, codigo, null, null);
    }
}
