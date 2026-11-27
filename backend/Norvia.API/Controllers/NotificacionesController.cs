using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Notificaciones.ContarNoLeidas;
using Norvia.Application.Features.Notificaciones.DTOs;
using Norvia.Application.Features.Notificaciones.ListarMisNotificaciones;
using Norvia.Application.Features.Notificaciones.ListarMisNotificaciones.DTOs;
using Norvia.Application.Features.Notificaciones.MarcarNotificacionLeida;
using Norvia.Application.Features.Notificaciones.ReintentarCorreo;

namespace Norvia.API.Controllers;

/// <summary>Módulo Notificaciones (M11): la bandeja de quien tiene la sesión (HU-117). Solo las propias (RN-79).</summary>
[ApiController]
[Route("api/notificaciones")]
public sealed class NotificacionesController : ControllerBase
{
    /// <summary>HU-117: mis notificaciones, de la más reciente a la más antigua, y cuántas no he leído.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(MisNotificacionesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] FiltrosNotificacionesDto filtros, [FromServices] ListarMisNotificacionesServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(filtros, cancellationToken)).AHttp(this);

    /// <summary>HU-117: cuántas no he leído (la campana del encabezado).</summary>
    [HttpGet("no-leidas")]
    [ProducesResponseType(typeof(NoLeidasResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> NoLeidas([FromServices] ContarNoLeidasServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(cancellationToken)).AHttp(this);

    /// <summary>HU-117: marca como leída una notificación propia. Repetirlo no cambia nada.</summary>
    [HttpPatch("{id:guid}/lectura")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarLeida(
        Guid id, [FromServices] MarcarNotificacionLeidaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>
    /// Reintenta de inmediato el correo de una notificación propia, en vez de esperar al poller de fondo (RN-78).
    /// </summary>
    [HttpPost("{id:guid}/reintentar-correo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReintentarCorreo(
        Guid id, [FromServices] ReintentarCorreoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);
}
