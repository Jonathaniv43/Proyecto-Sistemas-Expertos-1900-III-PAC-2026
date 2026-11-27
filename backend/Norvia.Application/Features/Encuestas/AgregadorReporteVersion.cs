using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas;

/// <summary>
/// Cálculo compartido de distribuciones anónimas y exposición por versión (HU-032, HU-034, RN-12, RN-73).
/// </summary>
internal static class AgregadorReporteVersion
{
    public const int MinimoParaDetalle = 3;

    public static List<PreguntaReporteResponseDto> ArmarPreguntas(
        EncuestaVersion version,
        IReadOnlyList<RespuestaEncuesta> respuestas,
        bool ocultarDetalle)
    {
        var ordenadas = version.Preguntas.OrderBy(p => p.Orden);
        if (ocultarDetalle)
        {
            return ordenadas
                .Select(p => new PreguntaReporteResponseDto(
                    p.Id,
                    p.Texto,
                    p.TipoRespuestaPregunta?.Codigo ?? string.Empty,
                    null,
                    null))
                .ToList();
        }

        return ordenadas
            .Select(p => ArmarPregunta(p, respuestas))
            .ToList();
    }

    public static PreguntaReporteResponseDto ArmarPregunta(
        EncuestaPregunta pregunta,
        IReadOnlyList<RespuestaEncuesta> respuestas)
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
            return new PreguntaReporteResponseDto(pregunta.Id, pregunta.Texto, codigo, null, textos);
        }

        if (TipoRespuestaPreguntaCatalogo.CodigoRequiereOpciones(codigo))
        {
            var distribucion = CalcularDistribucionOpciones(pregunta, codigo, detalles);
            return new PreguntaReporteResponseDto(pregunta.Id, pregunta.Texto, codigo, distribucion, null);
        }

        return new PreguntaReporteResponseDto(pregunta.Id, pregunta.Texto, codigo, null, null);
    }

    /// <summary>
    /// Cuenta cuántas respuestas distintas de la versión indican exposición para cada peligro asociado a preguntas.
    /// </summary>
    public static Dictionary<Guid, ExposicionPeligroVersion> CalcularExposicionesPorPeligro(
        EncuestaVersion version,
        IReadOnlyList<RespuestaEncuesta> respuestas)
    {
        var respuestasPorPeligro = new Dictionary<Guid, (string Nombre, HashSet<Guid> RespuestaIds)>();

        foreach (var pregunta in version.Preguntas)
        {
            if (pregunta.PeligroId is not Guid peligroId) continue;

            var opcionesExposicion = pregunta.Opciones
                .Where(o => o.IndicaExposicion)
                .Select(o => o.Id)
                .ToHashSet();
            if (opcionesExposicion.Count == 0) continue;

            var nombre = pregunta.Peligro?.Nombre ?? string.Empty;
            if (!respuestasPorPeligro.TryGetValue(peligroId, out var entrada))
            {
                entrada = (nombre, []);
                respuestasPorPeligro[peligroId] = entrada;
            }
            else if (string.IsNullOrEmpty(entrada.Nombre) && !string.IsNullOrEmpty(nombre))
            {
                entrada = (nombre, entrada.RespuestaIds);
                respuestasPorPeligro[peligroId] = entrada;
            }

            var codigo = TipoRespuestaPreguntaCatalogo.NormalizarCodigo(pregunta.TipoRespuestaPregunta?.Codigo ?? string.Empty);
            var esUnica = codigo == TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica;

            foreach (var respuesta in respuestas)
            {
                foreach (var detalle in respuesta.Detalles)
                {
                    if (detalle.EncuestaPreguntaId != pregunta.Id) continue;
                    var expuesta = esUnica
                        ? detalle.EncuestaOpcionId is Guid opId && opcionesExposicion.Contains(opId)
                        : detalle.Opciones.Any(o => opcionesExposicion.Contains(o.EncuestaOpcionId));
                    if (expuesta)
                        entrada.RespuestaIds.Add(respuesta.Id);
                }
            }
        }

        return respuestasPorPeligro.ToDictionary(
            kv => kv.Key,
            kv => new ExposicionPeligroVersion(kv.Key, kv.Value.Nombre, kv.Value.RespuestaIds.Count));
    }

    public static int ContarRespondieronPregunta(EncuestaPregunta pregunta, IReadOnlyList<RespuestaEncuesta> respuestas) =>
        respuestas.Sum(r => r.Detalles.Count(d => d.EncuestaPreguntaId == pregunta.Id));

    private static List<DistribucionOpcionResponseDto> CalcularDistribucionOpciones(
        EncuestaPregunta pregunta,
        string codigo,
        List<DetalleRespuesta> detalles)
    {
        var conteos = pregunta.Opciones.ToDictionary(o => o.Id, _ => 0);

        if (TipoRespuestaPreguntaCatalogo.NormalizarCodigo(codigo) == TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica)
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
            foreach (var o in detalles.SelectMany(d => d.Opciones))
            {
                if (conteos.ContainsKey(o.EncuestaOpcionId))
                    conteos[o.EncuestaOpcionId]++;
            }
        }

        var basePorcentaje = Math.Max(detalles.Count, 1);
        return pregunta.Opciones
            .OrderBy(o => o.Orden)
            .Select(o =>
            {
                var cantidad = conteos[o.Id];
                var porcentaje = Math.Round(100.0 * cantidad / basePorcentaje, 1);
                return new DistribucionOpcionResponseDto(o.Id, o.Texto, cantidad, porcentaje);
            })
            .ToList();
    }
}

internal sealed record ExposicionPeligroVersion(Guid PeligroId, string Nombre, int RespuestasConExposicion);
