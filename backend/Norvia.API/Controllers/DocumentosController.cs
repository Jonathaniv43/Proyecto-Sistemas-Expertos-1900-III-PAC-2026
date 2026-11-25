using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.GenerarDocumento;
using Norvia.Application.Features.Documentos.GenerarDocumento.DTOs;
using Norvia.Application.Features.Documentos.ListarDocumentos;
using Norvia.Application.Features.Documentos.ListarPlantillasDisponibles;
using Norvia.Application.Features.Documentos.ObtenerDocumento;

namespace Norvia.API.Controllers;

/// <summary>
/// Módulo Documentos (M9): políticas y procedimientos de la empresa (D13).
/// <para>
/// Pendiente de futuras HUs en este controlador: editar el borrador (HU-071), aprobar (HU-072), versión nueva (HU-073)
/// y vigentes (HU-074).
/// </para>
/// </summary>
[ApiController]
[Route("api/documentos")]
public sealed class DocumentosController : ControllerBase
{
    /// <summary>HU-070: plantillas activas del rubro de la empresa o de todos los rubros. Sin rubro en el perfil, 409.</summary>
    [HttpGet("plantillas")]
    [ProducesResponseType(typeof(IReadOnlyList<PlantillaDisponibleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Plantillas([FromServices] ListarPlantillasDisponiblesServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(cancellationToken)).AHttp(this);

    /// <summary>HU-070: genera un documento desde una plantilla, con los datos de la empresa; queda como borrador (RN-49).</summary>
    [HttpPost("generacion")]
    [ProducesResponseType(typeof(DocumentoDetalleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Generar(
        GenerarDocumentoRequestDto request, [FromServices] GenerarDocumentoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-070: los documentos de la empresa con el estado de su versión de trabajo, el último actualizado primero.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentoResumenResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromServices] ListarDocumentosServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(cancellationToken)).AHttp(this);

    /// <summary>HU-070: un documento con su versión de trabajo y lo que falta completar.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, [FromServices] ObtenerDocumentoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);
}
