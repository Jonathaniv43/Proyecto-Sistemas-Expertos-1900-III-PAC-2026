using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ResponderEncuesta.DTOs;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ResponderEncuesta;

/// <summary>
/// HU-031: registra la respuesta anónima y la participación del vínculo en una sola transacción (D27).
/// </summary>
public sealed class ResponderEncuestaServicio(
    IResponderEncuestaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<ResponderEncuestaResponseDto>> EjecutarAsync(
        Guid encuestaId, ResponderEncuestaRequestDto request, CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId || sesion.VinculoEmpresaId is not Guid vinculoId)
            return Errores.EncuestaNoEncontrada;

        var encuesta = await repositorio.BuscarConVersionPublicadaAsync(encuestaId, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        if (encuesta.Estado == EstadoEncuesta.Cerrada)
            return Errores.EncuestaCerrada;

        if (encuesta.Estado != EstadoEncuesta.Publicada)
            return Errores.EncuestaSinVersionPublicada;

        if (await repositorio.YaParticipoAsync(encuestaId, vinculoId, ct))
            return Errores.YaParticipo;

        var version = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Publicada);
        if (version is null)
            return Errores.EncuestaSinVersionPublicada;

        if (version.Id != request.VersionId || version.EncuestaId != encuestaId)
            return Errores.VersionObsoleta;

        var porPregunta = version.Preguntas.ToDictionary(p => p.Id);
        var enviadas = request.Respuestas.ToDictionary(r => r.PreguntaId);

        foreach (var pregunta in version.Preguntas.Where(p => p.Obligatoria))
        {
            if (!enviadas.ContainsKey(pregunta.Id))
                return Errores.RespuestaInvalidaDetalle(
                    $"Falta responder la pregunta obligatoria «{pregunta.Texto}».");
        }

        foreach (var item in request.Respuestas)
        {
            if (!porPregunta.ContainsKey(item.PreguntaId))
                return Errores.RespuestaInvalidaDetalle(
                    "Una de las respuestas no corresponde a una pregunta de esta versión.");
        }

        var ahora = await reloj.AhoraAsync(ct);
        var respuesta = new RespuestaEncuesta
        {
            EmpresaId = empresaId,
            EncuestaVersionId = version.Id,
            FechaRespuesta = DateOnly.FromDateTime(ahora)
        };

        foreach (var item in request.Respuestas)
        {
            var pregunta = porPregunta[item.PreguntaId];
            var error = ValidarYMapear(pregunta, item, respuesta, empresaId);
            if (error is not null)
                return error;
        }

        repositorio.Agregar(respuesta);
        repositorio.Agregar(new ParticipacionEncuesta
        {
            EmpresaId = empresaId,
            EncuestaId = encuestaId,
            VinculoEmpresaId = vinculoId
        });

        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return new ResponderEncuestaResponseDto("Su respuesta fue registrada.");
    }

    private static Error? ValidarYMapear(
        EncuestaPregunta pregunta,
        RespuestaPreguntaRequestDto item,
        RespuestaEncuesta respuesta,
        Guid empresaId)
    {
        var codigo = pregunta.TipoRespuestaPregunta?.Codigo ?? string.Empty;

        if (TipoRespuestaPreguntaCatalogo.EsCodigoRespuestaBreve(codigo))
        {
            if (item.OpcionId is not null || item.OpcionIds is { Count: > 0 })
                return Errores.RespuestaInvalidaDetalle(
                    $"La pregunta «{pregunta.Texto}» solo admite texto libre.");

            var texto = item.TextoLibre?.Trim() ?? string.Empty;
            if (pregunta.Obligatoria && texto.Length == 0)
                return Errores.RespuestaInvalidaDetalle(
                    $"La pregunta «{pregunta.Texto}» es obligatoria.");

            var max = pregunta.TipoRespuestaPregunta?.MaxCaracteresRespuesta ?? 500;
            if (texto.Length > max)
                return Errores.RespuestaInvalidaDetalle(
                    $"La pregunta «{pregunta.Texto}» admite como máximo {max} caracteres.");

            respuesta.Detalles.Add(new DetalleRespuesta
            {
                EmpresaId = empresaId,
                RespuestaEncuestaId = respuesta.Id,
                EncuestaPreguntaId = pregunta.Id,
                TextoLibre = texto.Length == 0 ? null : texto
            });
            return null;
        }

        if (TipoRespuestaPreguntaCatalogo.NormalizarCodigo(codigo)
            == TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica)
        {
            if (item.OpcionId is null || item.OpcionIds is { Count: > 0 }
                || !string.IsNullOrWhiteSpace(item.TextoLibre))
                return Errores.RespuestaInvalidaDetalle(
                    $"La pregunta «{pregunta.Texto}» requiere exactamente una opción.");

            if (pregunta.Opciones.All(o => o.Id != item.OpcionId))
                return Errores.RespuestaInvalidaDetalle(
                    $"La opción elegida no pertenece a la pregunta «{pregunta.Texto}».");

            respuesta.Detalles.Add(new DetalleRespuesta
            {
                EmpresaId = empresaId,
                RespuestaEncuestaId = respuesta.Id,
                EncuestaPreguntaId = pregunta.Id,
                EncuestaOpcionId = item.OpcionId
            });
            return null;
        }

        if (TipoRespuestaPreguntaCatalogo.NormalizarCodigo(codigo)
            == TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple)
        {
            if (item.OpcionIds is not { Count: > 0 } || item.OpcionId is not null
                || !string.IsNullOrWhiteSpace(item.TextoLibre))
                return Errores.RespuestaInvalidaDetalle(
                    $"La pregunta «{pregunta.Texto}» requiere una o más opciones.");

            if (item.OpcionIds.Distinct().Count() != item.OpcionIds.Count)
                return Errores.RespuestaInvalidaDetalle(
                    $"No envíe opciones repetidas en «{pregunta.Texto}».");

            var idsValidos = pregunta.Opciones.Select(o => o.Id).ToHashSet();
            if (item.OpcionIds.Any(id => !idsValidos.Contains(id)))
                return Errores.RespuestaInvalidaDetalle(
                    $"Una opción no pertenece a la pregunta «{pregunta.Texto}».");

            var detalle = new DetalleRespuesta
            {
                EmpresaId = empresaId,
                RespuestaEncuestaId = respuesta.Id,
                EncuestaPreguntaId = pregunta.Id
            };
            foreach (var opcionId in item.OpcionIds)
            {
                detalle.Opciones.Add(new DetalleRespuestaOpcion
                {
                    EmpresaId = empresaId,
                    RespuestaEncuestaId = respuesta.Id,
                    EncuestaPreguntaId = pregunta.Id,
                    EncuestaOpcionId = opcionId
                });
            }

            respuesta.Detalles.Add(detalle);
            return null;
        }

        return Errores.RespuestaInvalida;
    }
}
