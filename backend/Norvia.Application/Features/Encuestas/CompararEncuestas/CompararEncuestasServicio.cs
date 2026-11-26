using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.CompararEncuestas.DTOs;
using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.CompararEncuestas;

/// <summary>
/// HU-034: compara los resultados y la exposición a peligros entre dos versiones publicadas/cerradas
/// (de la misma encuesta o de encuestas distintas), respetando RN-12, D8, RN-72 y RN-73.
/// </summary>
public sealed class CompararEncuestasServicio(
    ICompararEncuestasRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<ComparacionEncuestasResponseDto>> EjecutarAsync(
        CompararEncuestasRequestDto request, CancellationToken ct)
    {
        if (request.BaseVersionId is not Guid baseId || request.CompararVersionId is not Guid compararId)
            return Errores.VersionNoComparable;

        if (baseId == compararId)
            return Errores.VersionNoComparable;

        var versionBase = await repositorio.BuscarVersionAsync(baseId, ct);
        if (versionBase is null)
            return Errores.VersionNoEncontrada;

        var versionComparar = await repositorio.BuscarVersionAsync(compararId, ct);
        if (versionComparar is null)
            return Errores.VersionNoEncontrada;

        if (versionBase.Estado == EstadoVersionEncuesta.Borrador
            || versionComparar.Estado == EstadoVersionEncuesta.Borrador)
            return Errores.VersionSinPublicar;

        var participacionesBase = await repositorio.ContarParticipacionesAsync(versionBase.EncuestaId, ct);
        var participacionesComparar = versionBase.EncuestaId == versionComparar.EncuestaId
            ? participacionesBase
            : await repositorio.ContarParticipacionesAsync(versionComparar.EncuestaId, ct);

        var respuestasBase = await repositorio.ListarRespuestasDeVersionAsync(versionBase.Id, ct);
        var respuestasComparar = await repositorio.ListarRespuestasDeVersionAsync(versionComparar.Id, ct);

        var ladoBase = ArmarLado(versionBase, participacionesBase, respuestasBase);
        var ladoComparar = ArmarLado(versionComparar, participacionesComparar, respuestasComparar);
        var ocultarComparacion = ladoBase.DetalleOculto || ladoComparar.DetalleOculto;

        if (ocultarComparacion)
        {
            return new ComparacionEncuestasResponseDto(
                ladoBase,
                ladoComparar,
                AgregadorReporteVersion.MinimoParaDetalle,
                ComparacionOculta: true,
                PreguntasComparadas: [],
                PreguntasSoloBase: [],
                PreguntasSoloComparar: [],
                PeligrosComparados: []);
        }

        var (comparadas, soloBase, soloComparar) = CompararPreguntas(
            versionBase, respuestasBase, versionComparar, respuestasComparar);

        var riesgosBase = await repositorio.EstadosRiesgosPorPeligroAsync(versionBase.EncuestaId, ct);
        var riesgosComparar = versionBase.EncuestaId == versionComparar.EncuestaId
            ? riesgosBase
            : await repositorio.EstadosRiesgosPorPeligroAsync(versionComparar.EncuestaId, ct);

        var peligros = CompararPeligros(
            versionBase, respuestasBase, riesgosBase,
            versionComparar, respuestasComparar, riesgosComparar);

        return new ComparacionEncuestasResponseDto(
            ladoBase,
            ladoComparar,
            AgregadorReporteVersion.MinimoParaDetalle,
            ComparacionOculta: false,
            comparadas,
            soloBase,
            soloComparar,
            peligros);
    }

    private static LadoComparacionDto ArmarLado(
        EncuestaVersion version,
        int participaciones,
        IReadOnlyList<RespuestaEncuesta> respuestas)
    {
        var totalRespuestas = respuestas.Count;
        var categorias = version.Categorias
            .Select(c => c.CategoriaRiesgo.Nombre)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var breves = version.Preguntas.Count(p =>
            TipoRespuestaPreguntaCatalogo.EsCodigoRespuestaBreve(p.TipoRespuestaPregunta?.Codigo ?? string.Empty));

        return new LadoComparacionDto(
            version.EncuestaId,
            version.Encuesta.Titulo,
            version.Encuesta.Estado,
            version.Id,
            version.NumeroVersion,
            version.Estado,
            version.PublicadaEn,
            version.Encuesta.CerradaEn,
            participaciones,
            totalRespuestas,
            totalRespuestas == 0 ? null : respuestas.Min(r => r.FechaRespuesta),
            totalRespuestas == 0 ? null : respuestas.Max(r => r.FechaRespuesta),
            totalRespuestas < AgregadorReporteVersion.MinimoParaDetalle,
            categorias,
            breves);
    }

    private static (
        List<PreguntaComparadaDto> Comparadas,
        List<PreguntaReporteResponseDto> SoloBase,
        List<PreguntaReporteResponseDto> SoloComparar) CompararPreguntas(
        EncuestaVersion versionBase,
        IReadOnlyList<RespuestaEncuesta> respuestasBase,
        EncuestaVersion versionComparar,
        IReadOnlyList<RespuestaEncuesta> respuestasComparar)
    {
        var seleccionBase = versionBase.Preguntas
            .OrderBy(p => p.Orden)
            .Where(EsDeSeleccion)
            .ToList();
        var seleccionComparar = versionComparar.Preguntas
            .OrderBy(p => p.Orden)
            .Where(EsDeSeleccion)
            .ToList();

        var usadasComparar = new HashSet<Guid>();
        var emparejadas = new Dictionary<Guid, (EncuestaPregunta Par, string Clave)>();

        // 1ª pasada: preguntas que conservan el mismo origen en catálogo y mismo tipo.
        foreach (var pBase in seleccionBase.Where(p => p.PreguntaExploratoriaId is not null))
        {
            var tipoBase = TipoNormalizado(pBase);
            var match = seleccionComparar.FirstOrDefault(p =>
                !usadasComparar.Contains(p.Id)
                && p.PreguntaExploratoriaId == pBase.PreguntaExploratoriaId
                && TipoNormalizado(p) == tipoBase);
            if (match is null) continue;

            usadasComparar.Add(match.Id);
            emparejadas[pBase.Id] = (match, "catalogo");
        }

        // 2ª pasada: preguntas editadas o personalizadas con el mismo enunciado normalizado y mismo tipo.
        foreach (var pBase in seleccionBase.Where(p => !emparejadas.ContainsKey(p.Id)))
        {
            var claveBase = Normalizar(pBase.Texto);
            var tipoBase = TipoNormalizado(pBase);
            var match = seleccionComparar.FirstOrDefault(p =>
                !usadasComparar.Contains(p.Id)
                && TipoNormalizado(p) == tipoBase
                && Normalizar(p.Texto) == claveBase);
            if (match is null) continue;

            usadasComparar.Add(match.Id);
            emparejadas[pBase.Id] = (match, "texto");
        }

        var comparadas = new List<PreguntaComparadaDto>();
        var soloBase = new List<PreguntaReporteResponseDto>();

        foreach (var pBase in seleccionBase)
        {
            var reporteBase = AgregadorReporteVersion.ArmarPregunta(pBase, respuestasBase);
            if (!emparejadas.TryGetValue(pBase.Id, out var parInfo))
            {
                soloBase.Add(reporteBase);
                continue;
            }

            var pComp = parInfo.Par;
            var reporteComp = AgregadorReporteVersion.ArmarPregunta(pComp, respuestasComparar);
            comparadas.Add(new PreguntaComparadaDto(
                parInfo.Clave,
                pBase.Texto,
                pComp.Texto,
                reporteBase.TipoRespuesta,
                AgregadorReporteVersion.ContarRespondieronPregunta(pBase, respuestasBase),
                AgregadorReporteVersion.ContarRespondieronPregunta(pComp, respuestasComparar),
                CompararOpciones(reporteBase.Distribucion ?? [], reporteComp.Distribucion ?? [])));
        }

        var soloComparar = seleccionComparar
            .Where(p => !usadasComparar.Contains(p.Id))
            .Select(p => AgregadorReporteVersion.ArmarPregunta(p, respuestasComparar))
            .ToList();

        return (comparadas, soloBase, soloComparar);
    }

    private static List<OpcionComparadaDto> CompararOpciones(
        IReadOnlyList<DistribucionOpcionResponseDto> distBase,
        IReadOnlyList<DistribucionOpcionResponseDto> distComparar)
    {
        var usadasComparar = new HashSet<Guid>();
        var lista = new List<OpcionComparadaDto>();

        foreach (var opBase in distBase)
        {
            var clave = Normalizar(opBase.Etiqueta);
            var opComp = distComparar.FirstOrDefault(o => !usadasComparar.Contains(o.OpcionId) && Normalizar(o.Etiqueta) == clave);
            if (opComp is not null)
                usadasComparar.Add(opComp.OpcionId);

            var cantComp = opComp?.Cantidad ?? 0;
            var pctComp = opComp?.Porcentaje ?? 0;
            lista.Add(new OpcionComparadaDto(
                opBase.Etiqueta,
                opBase.Cantidad,
                opBase.Porcentaje,
                cantComp,
                pctComp,
                Math.Round(pctComp - opBase.Porcentaje, 1)));
        }

        foreach (var opComp in distComparar.Where(o => !usadasComparar.Contains(o.OpcionId)))
        {
            lista.Add(new OpcionComparadaDto(
                opComp.Etiqueta,
                0,
                0,
                opComp.Cantidad,
                opComp.Porcentaje,
                opComp.Porcentaje));
        }

        return lista;
    }

    private static List<PeligroComparadoDto> CompararPeligros(
        EncuestaVersion versionBase,
        IReadOnlyList<RespuestaEncuesta> respuestasBase,
        Dictionary<Guid, EstadoRiesgo> riesgosBase,
        EncuestaVersion versionComparar,
        IReadOnlyList<RespuestaEncuesta> respuestasComparar,
        Dictionary<Guid, EstadoRiesgo> riesgosComparar)
    {
        var expBase = AgregadorReporteVersion.CalcularExposicionesPorPeligro(versionBase, respuestasBase);
        var expComp = AgregadorReporteVersion.CalcularExposicionesPorPeligro(versionComparar, respuestasComparar);

        var todosIds = expBase.Keys.Union(expComp.Keys);
        var totalBase = Math.Max(respuestasBase.Count, 1);
        var totalComp = Math.Max(respuestasComparar.Count, 1);

        return todosIds
            .Select(peligroId =>
            {
                expBase.TryGetValue(peligroId, out var b);
                expComp.TryGetValue(peligroId, out var c);
                var nombre = !string.IsNullOrEmpty(b?.Nombre) ? b.Nombre : c?.Nombre ?? string.Empty;
                var cantBase = b?.RespuestasConExposicion ?? 0;
                var cantComp = c?.RespuestasConExposicion ?? 0;
                var pctBase = Math.Round(100.0 * cantBase / totalBase, 1);
                var pctComp = Math.Round(100.0 * cantComp / totalComp, 1);

                return new PeligroComparadoDto(
                    peligroId,
                    nombre,
                    cantBase,
                    pctBase,
                    riesgosBase.TryGetValue(peligroId, out var eb) ? eb : null,
                    cantComp,
                    pctComp,
                    riesgosComparar.TryGetValue(peligroId, out var ec) ? ec : null,
                    Math.Round(pctComp - pctBase, 1));
            })
            .OrderByDescending(p => Math.Max(p.PorcentajeBase, p.PorcentajeComparar))
            .ThenBy(p => p.PeligroNombre, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool EsDeSeleccion(EncuestaPregunta p) =>
        TipoRespuestaPreguntaCatalogo.CodigoRequiereOpciones(p.TipoRespuestaPregunta?.Codigo ?? string.Empty);

    private static string TipoNormalizado(EncuestaPregunta p) =>
        TipoRespuestaPreguntaCatalogo.NormalizarCodigo(p.TipoRespuestaPregunta?.Codigo ?? string.Empty);

    private static string Normalizar(string texto) => texto.Trim().ToLowerInvariant();
}
