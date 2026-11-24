using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Eventos.ListarEventos;
using Norvia.Application.Features.Eventos.ListarEventos.DTOs;
using Norvia.Application.Features.Eventos.ObtenerEvento;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Application.Features.Eventos.ReportarEvento;
using Norvia.Application.Features.Eventos.ReportarEvento.DTOs;
using Norvia.Application.Features.Eventos.CerrarEvento;
using Norvia.Application.Features.Eventos.CrearMedidaCorrectiva;
using Norvia.Application.Features.Eventos.CrearMedidaCorrectiva.DTOs;
using Norvia.Application.Features.Eventos.ListarMedidasCorrectivas;
using Norvia.Application.Features.Eventos.ListarRiesgosDelEvento;
using Norvia.Application.Features.Eventos.ListarRiesgosDelEvento.DTOs;
using Norvia.Application.Features.Eventos.DescartarEvento;
using Norvia.Application.Features.Eventos.DescartarEvento.DTOs;
using Norvia.Application.Features.Eventos.RegistrarInvestigacion;
using Norvia.Application.Features.Eventos.RegistrarInvestigacion.DTOs;

namespace Norvia.API.Controllers;

/// <summary>
/// Módulo Eventos / Accidentes (M6).
/// <para>
/// Pendiente de futuras HUs en este controlador:
/// - HU-100: <c>POST /api/eventos/{id}/evidencias</c> (adjuntos en almacenamiento externo).
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

    /// <summary>HU-049: registra o corrige la investigación (causas, conclusiones, quién y cuándo); el evento queda en investigación.</summary>
    [HttpPut("{id:guid}/investigacion")]
    [ProducesResponseType(typeof(EventoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Investigar(
        Guid id, RegistrarInvestigacionRequestDto request, [FromServices] RegistrarInvestigacionServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    /// <summary>HU-049: cierra el evento; exige investigación y al menos una medida correctiva (RN-28).</summary>
    [HttpPost("{id:guid}/cierre")]
    [ProducesResponseType(typeof(EventoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cerrar(Guid id, [FromServices] CerrarEventoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-049: descarta un reporte falso, duplicado o inválido, con motivo opcional (RN-28).</summary>
    [HttpPatch("{id:guid}/descarte")]
    [ProducesResponseType(typeof(EventoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Descartar(
        Guid id, DescartarEventoRequestDto request, [FromServices] DescartarEventoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    /// <summary>HU-050: las medidas correctivas del evento (tareas del plan con origen en él) con su avance.</summary>
    [HttpGet("{id:guid}/medidas-correctivas")]
    [ProducesResponseType(typeof(IReadOnlyList<MedidaCorrectivaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarMedidas(
        Guid id, [FromServices] ListarMedidasCorrectivasServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-050: agrega una medida correctiva, que queda como tarea del plan con origen en el evento (RN-23).</summary>
    [HttpPost("{id:guid}/medidas-correctivas")]
    [ProducesResponseType(typeof(MedidaCorrectivaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearMedida(
        Guid id, CrearMedidaCorrectivaRequestDto request, [FromServices] CrearMedidaCorrectivaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-051: los riesgos que el evento reveló y los que se reevaluaron a raíz de él, con el nivel de antes y después.</summary>
    [HttpGet("{id:guid}/riesgos")]
    [ProducesResponseType(typeof(IReadOnlyList<RiesgoDelEventoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarRiesgos(
        Guid id, [FromServices] ListarRiesgosDelEventoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);
}
