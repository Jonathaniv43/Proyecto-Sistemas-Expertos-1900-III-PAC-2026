using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.CrearPregunta.DTOs;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPregunta;

/// <summary>Crea una pregunta exploratoria con su tipo y sus opciones (D7, D26).</summary>
public sealed class CrearPreguntaServicio(ICrearPreguntaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<PreguntaResponseDto>> EjecutarAsync(CrearPreguntaRequestDto request, CancellationToken ct)
    {
        if (!await repositorio.CategoriaActivaExisteAsync(request.CategoriaRiesgoId, ct)) return Errores.CategoriaInvalida;
        if (request.PeligroId is Guid peligroId && !await repositorio.PeligroActivoExisteAsync(peligroId, ct))
            return Errores.PeligroInvalido;

        var codigo = TipoRespuestaPreguntaCatalogo.NormalizarCodigo(request.TipoRespuestaCodigo);
        var tipoId = await repositorio.BuscarTipoIdPorCodigoAsync(codigo, ct);
        if (tipoId is null) return Errores.TipoRespuestaInvalido;

        var pregunta = new PreguntaExploratoria
        {
            CategoriaRiesgoId = request.CategoriaRiesgoId,
            Texto = request.Texto.Trim(),
            TipoRespuestaPreguntaId = tipoId.Value,
            Obligatoria = request.Obligatoria,
            Orden = (short)(await repositorio.UltimoOrdenAsync(request.CategoriaRiesgoId, ct) + 1),
            PeligroId = request.PeligroId,
            Activa = true
        };
        foreach (var o in request.Opciones!)
            pregunta.Opciones.Add(new OpcionPregunta
            {
                PreguntaExploratoriaId = pregunta.Id,
                Texto = o.Texto.Trim(),
                Orden = o.Orden,
                IndicaExposicion = o.IndicaExposicion
            });

        repositorio.Agregar(pregunta);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return PreguntaResponseDto.De(pregunta);
    }
}
