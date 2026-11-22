using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Eventos.ListarEventos;
using Norvia.Application.Features.Eventos.ListarEventos.DTOs;
using Norvia.Application.Features.Eventos.ObtenerEvento;
using Norvia.Application.Features.Eventos.ObtenerEvento.DTOs;
using Norvia.Application.Features.Eventos.ReportarEvento;
using Norvia.Application.Features.Eventos.ReportarEvento.DTOs;

namespace Norvia.API.Controllers;

/// <summary>
/// Módulo Eventos / Accidentes (M6).
/// <para>
/// Pendiente de futuras HUs en este controlador:
/// - HU-100: <c>POST /api/eventos/{id}/evidencias</c> (adjuntos en almacenamiento externo).
/// - HU-049: <c>PATCH /api/eventos/{id}/investigacion</c> y <c>PATCH /api/eventos/{id}/descarte</c>.
/// - HU-050: <c>POST /api/eventos/{id}/medidas-correctivas</c>.
/// - HU-052: <c>GET /api/eventos/estadisticas</c>.
/// </para>
/// </summary>
[ApiController]
[Route("api/eventos")]
public sealed class EventosController : ControllerBase
{
    /// <summary>HU-047: reporta de forma anónima un accidente, incidente, enfermedad profesional o condición insegura (RN-26, RN-27, D11).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReportarEventoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reportar(
        ReportarEventoRequestDto request, [FromServices] ReportarEventoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-047: listado resumido de eventos de la empresa, con filtros opcionales en snake_case (?estado=&amp;tipo=&amp;esGrave=).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventoResumenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado, [FromQuery] string? tipo, [FromQuery] bool? esGrave,
        [FromServices] ListarEventosServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(estado, tipo, esGrave, cancellationToken)).AHttp(this);

    /// <summary>HU-047: detalle completo de un evento reportado en la empresa.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(
        Guid id, [FromServices] ObtenerEventoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);
}
