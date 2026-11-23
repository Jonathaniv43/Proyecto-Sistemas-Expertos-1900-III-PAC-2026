using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos;

/// <summary>
/// HU-033: sugiere peligros a partir de las respuestas de la encuesta (RN-13, D9).
/// <list type="bullet">
/// <item>Un peligro se sugiere si al menos una respuesta eligió una opción de exposición en una pregunta con ese peligro.</item>
/// <item>Se calcula sobre las respuestas de todas las versiones, cada una con su propia copia de preguntas y opciones:
/// si una versión nueva cambia la relación pregunta-peligro, sus respuestas sugieren el peligro nuevo.</item>
/// <item>Cada peligro sugerido se registra una sola vez como <c>RiesgoEmpresa</c> en estado <c>sugerido</c> (UQ EncuestaId+PeligroId):
/// las mismas respuestas dan las mismas sugerencias (RN-14) y las nuevas no las repiten. Uno ya confirmado o
/// descartado (HU-035) no se toca.</item>
/// <item>El respaldo (cuántas respuestas) se calcula al consultar; no se guarda (3FN).</item>
/// <item>Se muestran y registran con cualquier cantidad de respuestas.</item>
/// </list>
/// </summary>
public sealed class ObtenerPeligrosSugeridosServicio(
    IObtenerPeligrosSugeridosRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PeligrosSugeridosResponseDto>> EjecutarAsync(Guid encuestaId, CancellationToken ct)
    {
        if (!await repositorio.ExisteEncuestaAsync(encuestaId, ct)) return Errores.EncuestaNoEncontrada;

        var total = await repositorio.ContarRespuestasAsync(encuestaId, ct);

        var exposiciones = await repositorio.ListarExposicionesAsync(encuestaId, ct);
        var porPeligro = exposiciones.GroupBy(e => e.PeligroId).ToList();
        if (porPeligro.Count == 0)
            return new PeligrosSugeridosResponseDto(encuestaId, total, 0, DetalleOculto: false, []);

        // Registrar las sugerencias que falten, sin duplicar ni tocar las ya revisadas.
        var riesgos = (await repositorio.ListarRiesgosDeEncuestaAsync(encuestaId, ct))
            .Where(r => r.PeligroId is not null)
            .ToDictionary(r => r.PeligroId!.Value);
        var nuevos = false;
        foreach (var grupo in porPeligro.Where(g => !riesgos.ContainsKey(g.Key)))
        {
            var riesgo = new RiesgoEmpresa { PeligroId = grupo.Key, EncuestaId = encuestaId, Estado = EstadoRiesgo.Sugerido };
            repositorio.AgregarRiesgo(riesgo);
            riesgos[grupo.Key] = riesgo;
            nuevos = true;
        }
        if (nuevos) await unidadDeTrabajo.GuardarCambiosAsync(ct);

        var nombres = await repositorio.NombresDePeligrosAsync(porPeligro.Select(g => g.Key).ToList(), ct);
        var sugerencias = porPeligro
            .Select(g => new PeligroSugeridoDto(
                riesgos[g.Key].Id,
                g.Key,
                nombres.GetValueOrDefault(g.Key, string.Empty),
                riesgos[g.Key].Estado,
                // Una respuesta cuenta una vez por peligro, aunque lo indique en varias preguntas.
                g.Select(e => e.RespuestaId).Distinct().Count(),
                g.GroupBy(e => (e.PreguntaId, e.PreguntaTexto))
                    .Select(p => new PreguntaOrigenDto(p.Key.PreguntaId, p.Key.PreguntaTexto,
                        p.Select(e => e.RespuestaId).Distinct().Count()))
                    .OrderByDescending(p => p.RespuestasConExposicion).ThenBy(p => p.Texto)
                    .ToList()))
            .OrderByDescending(s => s.RespuestasConExposicion).ThenBy(s => s.PeligroNombre)
            .ToList();

        return new PeligrosSugeridosResponseDto(encuestaId, total, 0, DetalleOculto: false, sugerencias);
    }
}
