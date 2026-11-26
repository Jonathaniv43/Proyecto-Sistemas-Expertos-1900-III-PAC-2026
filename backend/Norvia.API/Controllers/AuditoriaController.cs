using Microsoft.AspNetCore.Mvc;
using Norvia.Application.Features.Auditoria.ListarAuditoria;
using Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;

namespace Norvia.API.Controllers;

/// <summary>HU-099: consulta cronológica de solo lectura del registro inalterable de auditoría de la empresa.</summary>
[ApiController]
[Route("api/auditoria")]
public sealed class AuditoriaController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AuditoriaPaginadaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] FiltrosAuditoriaDto filtros,
        [FromServices] ListarAuditoriaServicio servicio,
        CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(filtros, cancellationToken));
}
