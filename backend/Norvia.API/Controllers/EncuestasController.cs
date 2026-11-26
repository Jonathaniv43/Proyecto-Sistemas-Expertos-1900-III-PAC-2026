using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Encuestas.CompararEncuestas;
using Norvia.Application.Features.Encuestas.CompararEncuestas.DTOs;
using Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos;
using Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos.DTOs;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.CerrarEncuesta;
using Norvia.Application.Features.Encuestas.CrearEncuesta;
using Norvia.Application.Features.Encuestas.CrearEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles;
using Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles.DTOs;
using Norvia.Application.Features.Encuestas.ListarEncuestas;
using Norvia.Application.Features.Encuestas.ListarEncuestas.DTOs;
using Norvia.Application.Features.Encuestas.ListarEncuestasPendientes;
using Norvia.Application.Features.Encuestas.ListarEncuestasPendientes.DTOs;
using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas;
using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;
using Norvia.Application.Features.Encuestas.ListarTiposRespuesta;
using Norvia.Application.Features.Encuestas.ListarTiposRespuesta.DTOs;
using Norvia.Application.Features.Encuestas.ObtenerEncuesta;
using Norvia.Application.Features.Encuestas.ObtenerEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta;
using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.PublicarVersionEncuesta;
using Norvia.Application.Features.Encuestas.ResponderEncuesta;
using Norvia.Application.Features.Encuestas.ResponderEncuesta.DTOs;
using Norvia.Application.Features.Encuestas.RevisarPeligroSugerido;
using Norvia.Application.Features.Encuestas.RevisarPeligroSugerido.DTOs;

namespace Norvia.API.Controllers;

/// <summary>
/// Módulo Encuestas (M3).
/// <para>
/// El <c>PUT .../borrador</c> reemplaza el estado completo del borrador (secciones + preguntas).
/// Publicar: <c>POST .../versiones/{versionId}/publicacion</c> sin body; si ya hay publicada, la marca reemplazada (RN-70).
/// </para>
/// </summary>
[ApiController]
[Route("api/encuestas")]
public sealed class EncuestasController : ControllerBase
{
    /// <summary>HU-029: categorías del rubro de la empresa, con la vista previa de sus preguntas.</summary>
    [HttpGet("categorias-disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDisponibleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ListarCategoriasDisponibles(
        [FromServices] ListarCategoriasDisponiblesServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(cancellationToken)).AHttp(this);

    /// <summary>Tipos de respuesta con instrucción UI para el editor.</summary>
    [HttpGet("tipos-respuesta")]
    [ProducesResponseType(typeof(IReadOnlyList<TipoRespuestaPreguntaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTiposRespuesta(
        [FromServices] ListarTiposRespuestaServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>HU-031: encuestas publicadas que el vínculo de la sesión aún no ha respondido.</summary>
    [HttpGet("pendientes")]
    [ProducesResponseType(typeof(IReadOnlyList<EncuestaPendienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPendientes(
        [FromServices] ListarEncuestasPendientesServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>HU-030: listado de encuestas de la empresa.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EncuestaResumenResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromServices] ListarEncuestasServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>HU-034: encuestas cerradas o con versiones anteriores (y vigentes si se solicitan para comparar).</summary>
    [HttpGet("historial")]
    [ProducesResponseType(typeof(IReadOnlyList<EncuestaHistorialResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarHistorial(
        [FromQuery] FiltrosHistorialEncuestasDto filtros,
        [FromServices] ListarHistorialEncuestasServicio servicio,
        CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(filtros, cancellationToken));

    /// <summary>HU-034: compara resultados y peligros sugeridos entre dos versiones publicadas o cerradas.</summary>
    [HttpGet("comparacion")]
    [ProducesResponseType(typeof(ComparacionEncuestasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Comparar(
        [FromQuery] CompararEncuestasRequestDto request,
        [FromServices] CompararEncuestasServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>HU-030: detalle de una encuesta con su versión publicada y su borrador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EncuestaDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(
        Guid id, [FromServices] ObtenerEncuestaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-030: crea una encuesta en borrador copiando las preguntas activas de las categorías elegidas.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CrearEncuestaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        CrearEncuestaRequestDto request, [FromServices] CrearEncuestaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-030: edita el borrador de la encuesta o crea una nueva versión borrador si ya está publicada.</summary>
    [HttpPut("{id:guid}/borrador")]
    [ProducesResponseType(typeof(EncuestaVersionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarBorrador(
        Guid id, ActualizarEncuestaBorradorRequestDto request,
        [FromServices] ActualizarBorradorEncuestaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    /// <summary>HU-030: publica la versión en borrador y marca la publicada anterior como reemplazada.</summary>
    [HttpPost("{id:guid}/versiones/{versionId:guid}/publicacion")]
    [ProducesResponseType(typeof(EncuestaVersionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PublicarVersion(
        Guid id, Guid versionId,
        [FromServices] PublicarVersionEncuestaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, versionId, cancellationToken)).AHttp(this);

    /// <summary>HU-030: cierra la encuesta para impedir nuevas ediciones o respuestas.</summary>
    [HttpPost("{id:guid}/cierre")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cerrar(
        Guid id, [FromServices] CerrarEncuestaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-031: envía la respuesta anónima a la versión publicada.</summary>
    [HttpPost("{id:guid}/respuestas")]
    [ProducesResponseType(typeof(ResponderEncuestaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Responder(
        Guid id, ResponderEncuestaRequestDto request,
        [FromServices] ResponderEncuestaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    /// <summary>HU-032 y HU-034: reporte anónimo de la versión indicada o de la versión publicada vigente (RN-73).</summary>
    [HttpGet("{id:guid}/reporte")]
    [ProducesResponseType(typeof(ReporteEncuestaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ObtenerReporte(
        Guid id,
        [FromQuery] Guid? versionId,
        [FromServices] ObtenerReporteEncuestaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, versionId, cancellationToken)).AHttp(this);

    /// <summary>HU-033: peligros sugeridos por las respuestas (registra los nuevos como riesgos sugeridos).</summary>
    [HttpGet("{id:guid}/peligros-sugeridos")]
    [ProducesResponseType(typeof(PeligrosSugeridosResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPeligrosSugeridos(
        Guid id, [FromServices] ObtenerPeligrosSugeridosServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-035: confirma o descarta un peligro sugerido (<c>riesgoId</c> = <c>riesgoSugeridoId</c> de HU-033).</summary>
    [HttpPatch("{id:guid}/peligros-sugeridos/{riesgoId:guid}")]
    [ProducesResponseType(typeof(PeligroSugeridoRevisadoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevisarPeligroSugerido(
        Guid id, Guid riesgoId, RevisarPeligroSugeridoRequestDto request,
        [FromServices] RevisarPeligroSugeridoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, riesgoId, request, cancellationToken)).AHttp(this);
}
