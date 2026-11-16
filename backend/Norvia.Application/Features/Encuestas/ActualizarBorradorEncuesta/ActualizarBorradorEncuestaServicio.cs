using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;

/// <summary>
/// HU-030: actualiza la versión en borrador de una encuesta. Si la encuesta ya está publicada
/// y aún no tiene borrador, crea una versión nueva en borrador dejando intacta la vigente (RN-12).
/// </summary>
public sealed class ActualizarBorradorEncuestaServicio(
    IActualizarBorradorEncuestaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<EncuestaVersionResponseDto>> EjecutarAsync(
        Guid encuestaId, ActualizarEncuestaBorradorRequestDto request, CancellationToken ct)
    {
        var encuesta = await repositorio.BuscarConVersionesAsync(encuestaId, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        if (sesion.VinculoEmpresaId is not Guid vinculoId || encuesta.CreadaPorId != vinculoId)
            return Errores.SoloCreadorPuedeEditar;

        if (encuesta.Estado == EstadoEncuesta.Cerrada)
            return Errores.EncuestaCerrada;

        var borrador = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Borrador);
        var publicada = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Publicada);
        var baseVersion = borrador ?? publicada;
        if (baseVersion is null)
            return Errores.VersionNoEncontrada;

        var origenes = baseVersion.Preguntas.ToDictionary(p => p.Id);

        var peligrosNuevos = request.Preguntas
            .Where(q => q.PeligroId is Guid pId
                        && (!q.Id.HasValue || !origenes.TryGetValue(q.Id.Value, out var orig) || orig.PeligroId != pId))
            .Select(q => q.PeligroId!.Value)
            .Distinct()
            .ToList();
        if (peligrosNuevos.Count > 0 && !await repositorio.ExistenPeligrosActivosAsync(peligrosNuevos, ct))
            return Errores.PeligroInvalido;

        if (request.Titulo is not null)
            encuesta.Titulo = request.Titulo.Trim();

        var instrucciones = string.IsNullOrWhiteSpace(request.Instrucciones) ? null : request.Instrucciones.Trim();

        if (borrador is not null)
        {
            borrador.Instrucciones = instrucciones;
            var nuevasPreguntas = ConstruirPreguntas(borrador.EmpresaId, borrador.Id, request.Preguntas, origenes);
            repositorio.ReemplazarPreguntas(borrador, nuevasPreguntas);
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
            return EncuestaVersionResponseDto.De(borrador);
        }

        // La encuesta está publicada y no tiene borrador: se crea la versión N+1 sin alterar la publicada.
        var siguienteNumero = encuesta.Versiones.Max(v => v.NumeroVersion) + 1;
        var nuevaVersion = new EncuestaVersion
        {
            EmpresaId = encuesta.EmpresaId,
            EncuestaId = encuesta.Id,
            NumeroVersion = siguienteNumero,
            Instrucciones = instrucciones,
            Estado = EstadoVersionEncuesta.Borrador,
            CreadaPorId = vinculoId
        };

        foreach (var cat in baseVersion.Categorias)
        {
            nuevaVersion.Categorias.Add(new EncuestaCategoria
            {
                EmpresaId = encuesta.EmpresaId,
                EncuestaVersionId = nuevaVersion.Id,
                CategoriaRiesgoId = cat.CategoriaRiesgoId
            });
        }

        foreach (var pregunta in ConstruirPreguntas(encuesta.EmpresaId, nuevaVersion.Id, request.Preguntas, origenes))
            nuevaVersion.Preguntas.Add(pregunta);

        repositorio.AgregarVersion(nuevaVersion);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return EncuestaVersionResponseDto.De(nuevaVersion);
    }

    private static List<EncuestaPregunta> ConstruirPreguntas(
        Guid empresaId,
        Guid versionId,
        IReadOnlyList<PreguntaBorradorRequestDto> preguntasDto,
        Dictionary<Guid, EncuestaPregunta> origenes)
    {
        var lista = new List<EncuestaPregunta>(preguntasDto.Count);
        foreach (var q in preguntasDto.OrderBy(p => p.Orden))
        {
            Guid? preguntaOrigenId = q.Id.HasValue && origenes.TryGetValue(q.Id.Value, out var orig)
                ? orig.PreguntaExploratoriaId
                : null;

            var pregunta = new EncuestaPregunta
            {
                EmpresaId = empresaId,
                EncuestaVersionId = versionId,
                PreguntaExploratoriaId = preguntaOrigenId,
                Texto = q.Texto.Trim(),
                TipoRespuesta = q.TipoRespuesta!.Value,
                Obligatoria = q.Obligatoria,
                PeligroId = q.PeligroId,
                Orden = q.Orden
            };

            foreach (var o in q.Opciones.OrderBy(op => op.Orden))
            {
                pregunta.Opciones.Add(new EncuestaOpcion
                {
                    EmpresaId = empresaId,
                    EncuestaPreguntaId = pregunta.Id,
                    Texto = o.Texto.Trim(),
                    Orden = o.Orden,
                    IndicaExposicion = o.IndicaExposicion
                });
            }

            lista.Add(pregunta);
        }

        return lista;
    }
}
