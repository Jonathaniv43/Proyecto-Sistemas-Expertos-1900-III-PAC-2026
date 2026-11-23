using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;

/// <summary>
/// HU-030: actualiza la versión en borrador de una encuesta (estado completo: secciones + preguntas).
/// Si la encuesta ya está publicada y aún no tiene borrador, crea una versión nueva (RN-12).
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
        var tipos = await repositorio.TiposPorCodigoAsync(ct);

        foreach (var q in request.Preguntas)
        {
            var codigo = TipoRespuestaPreguntaCatalogo.NormalizarCodigo(q.TipoRespuestaCodigo);
            if (!tipos.ContainsKey(codigo))
                return Errores.TipoRespuestaInvalido;
        }

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
            AplicarContenido(borrador, request, origenes, tipos, reemplazar: true);
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
            return EncuestaVersionResponseDto.De(borrador);
        }

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

        AplicarContenido(nuevaVersion, request, origenes, tipos, reemplazar: false);
        repositorio.AgregarVersion(nuevaVersion);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return EncuestaVersionResponseDto.De(nuevaVersion);
    }

    private void AplicarContenido(
        EncuestaVersion version,
        ActualizarEncuestaBorradorRequestDto request,
        Dictionary<Guid, EncuestaPregunta> origenes,
        IReadOnlyDictionary<string, TipoRespuestaPreguntaCatalogo> tipos,
        bool reemplazar)
    {
        // Al crear versión nueva no se reutilizan Ids de la publicada (chocan con PK).
        // Al reemplazar el borrador actual sí se pueden reutilizar Ids del propio borrador.
        var (secciones, mapaSecciones) = ConstruirSecciones(
            version.EmpresaId, version.Id, request.Secciones, forzarIdsNuevos: !reemplazar);
        var preguntas = ConstruirPreguntas(
            version.EmpresaId, version.Id, request.Preguntas, origenes, tipos, mapaSecciones);

        if (reemplazar)
        {
            repositorio.ReemplazarSecciones(version, secciones);
            repositorio.ReemplazarPreguntas(version, preguntas);
        }
        else
        {
            foreach (var s in secciones) version.Secciones.Add(s);
            foreach (var p in preguntas) version.Preguntas.Add(p);
        }
    }

    private static (List<EncuestaSeccion> Secciones, IReadOnlyDictionary<Guid, Guid> MapaIds)
        ConstruirSecciones(
            Guid empresaId,
            Guid versionId,
            IReadOnlyList<SeccionBorradorRequestDto> seccionesDto,
            bool forzarIdsNuevos)
    {
        var lista = new List<EncuestaSeccion>(seccionesDto.Count);
        var mapa = new Dictionary<Guid, Guid>();
        foreach (var s in seccionesDto.OrderBy(x => x.Orden))
        {
            var nuevoId = forzarIdsNuevos || s.Id is null ? Guid.CreateVersion7() : s.Id.Value;
            if (s.Id is Guid idAnterior)
                mapa[idAnterior] = nuevoId;

            lista.Add(new EncuestaSeccion
            {
                Id = nuevoId,
                EmpresaId = empresaId,
                EncuestaVersionId = versionId,
                Titulo = s.Titulo.Trim(),
                Orden = s.Orden
            });
        }
        return (lista, mapa);
    }

    private static List<EncuestaPregunta> ConstruirPreguntas(
        Guid empresaId,
        Guid versionId,
        IReadOnlyList<PreguntaBorradorRequestDto> preguntasDto,
        Dictionary<Guid, EncuestaPregunta> origenes,
        IReadOnlyDictionary<string, TipoRespuestaPreguntaCatalogo> tipos,
        IReadOnlyDictionary<Guid, Guid> mapaSecciones)
    {
        var lista = new List<EncuestaPregunta>(preguntasDto.Count);
        foreach (var q in preguntasDto.OrderBy(p => p.Orden))
        {
            var codigo = TipoRespuestaPreguntaCatalogo.NormalizarCodigo(q.TipoRespuestaCodigo);
            var tipo = tipos[codigo];

            Guid? preguntaOrigenId = q.Id.HasValue && origenes.TryGetValue(q.Id.Value, out var orig)
                ? orig.PreguntaExploratoriaId
                : null;
            Guid? categoriaId = q.CategoriaRiesgoId
                ?? (q.Id.HasValue && origenes.TryGetValue(q.Id.Value, out var o2) ? o2.CategoriaRiesgoId : null);

            Guid? seccionId = q.SeccionId;
            if (seccionId is Guid sid && mapaSecciones.TryGetValue(sid, out var seccionNueva))
                seccionId = seccionNueva;

            var pregunta = new EncuestaPregunta
            {
                EmpresaId = empresaId,
                EncuestaVersionId = versionId,
                EncuestaSeccionId = seccionId,
                CategoriaRiesgoId = categoriaId,
                PreguntaExploratoriaId = preguntaOrigenId,
                Texto = q.Texto.Trim(),
                TipoRespuestaPreguntaId = tipo.Id,
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
