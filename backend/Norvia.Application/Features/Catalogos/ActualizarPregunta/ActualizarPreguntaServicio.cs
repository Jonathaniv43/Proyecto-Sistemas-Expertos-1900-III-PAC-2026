using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarPregunta.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarPregunta;

/// <summary>Edita, reordena o mueve de categoría una pregunta exploratoria.</summary>
public sealed class ActualizarPreguntaServicio(
    IActualizarPreguntaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PreguntaResponseDto>> EjecutarAsync(
        Guid id, ActualizarPreguntaRequestDto request, CancellationToken ct)
    {
        var pregunta = await repositorio.BuscarAsync(id, ct);
        if (pregunta is null) return Errores.PreguntaNoEncontrada;

        // Las referencias solo se exigen activas si cambian; así se puede editar algo que ya apuntaba a un inactivo.
        var categoriaId = request.CategoriaRiesgoId ?? pregunta.CategoriaRiesgoId;
        var cambiaCategoria = categoriaId != pregunta.CategoriaRiesgoId;
        if (cambiaCategoria && !await repositorio.CategoriaActivaExisteAsync(categoriaId, ct))
            return Errores.CategoriaInvalida;
        if (request.CambiaRespuesta && request.PeligroId is Guid peligroId && peligroId != pregunta.PeligroId
            && !await repositorio.PeligroActivoExisteAsync(peligroId, ct))
            return Errores.PeligroInvalido;

        short orden;
        if (request.Orden is short pedido)
        {
            if (await repositorio.OrdenOcupadoAsync(categoriaId, pedido, id, ct)) return Errores.OrdenOcupado;
            orden = pedido;
        }
        else
        {
            orden = cambiaCategoria ? (short)(await repositorio.UltimoOrdenAsync(categoriaId, ct) + 1) : pregunta.Orden;
        }

        pregunta.CategoriaRiesgoId = categoriaId;
        pregunta.Orden = orden;
        if (request.Texto is not null) pregunta.Texto = request.Texto.Trim();
        if (request.Obligatoria is bool obligatoria) pregunta.Obligatoria = obligatoria;

        if (request.CambiaRespuesta)
        {
            // El validador garantiza que vienen el tipo y las opciones.
            pregunta.TipoRespuesta = request.TipoRespuesta!.Value;
            pregunta.PeligroId = request.PeligroId;
            repositorio.ReemplazarOpciones(pregunta, request.Opciones!.Select(o => new OpcionPregunta
            {
                PreguntaExploratoriaId = pregunta.Id,
                Texto = o.Texto.Trim(),
                Orden = o.Orden,
                IndicaExposicion = o.IndicaExposicion
            }));
        }

        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return PreguntaResponseDto.De(pregunta);
    }
}
