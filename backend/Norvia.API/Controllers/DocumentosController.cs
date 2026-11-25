using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Documentos.AprobarDocumento;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.EditarBorrador;
using Norvia.Application.Features.Documentos.EditarBorrador.DTOs;
using Norvia.Application.Features.Documentos.GenerarDocumento;
using Norvia.Application.Features.Documentos.GenerarDocumento.DTOs;
using Norvia.Application.Features.Documentos.ListarDocumentos;
using Norvia.Application.Features.Documentos.ListarPlantillasDisponibles;
using Norvia.Application.Features.Documentos.ListarVigentes;
using Norvia.Application.Features.Documentos.ListarVigentes.DTOs;
using Norvia.Application.Features.Documentos.ObtenerVigente;
using Norvia.Application.Features.Documentos.ObtenerDocumento;

namespace Norvia.API.Controllers;

/// <summary>
/// Módulo Documentos (M9): políticas y procedimientos de la empresa (D13).
/// <para>
/// Pendiente de futuras HUs en este controlador: versión nueva (HU-073).
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

    /// <summary>HU-071: reemplaza el contenido del borrador. Solo el borrador se edita (RN-50); si no hay, 409.</summary>
    [HttpPut("{id:guid}/borrador")]
    [ProducesResponseType(typeof(DocumentoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EditarBorrador(
        Guid id, EditarBorradorRequestDto request, [FromServices] EditarBorradorServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    /// <summary>
    /// HU-072: aprueba el borrador y lo deja vigente; la vigente anterior pasa a reemplazada (RN-51). Si es una política,
    /// avisa a los vínculos activos (RN-75). Sin borrador o con datos por completar, 409.
    /// </summary>
    [HttpPost("{id:guid}/aprobacion")]
    [ProducesResponseType(typeof(DocumentoDetalleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Aprobar(Guid id, [FromServices] AprobarDocumentoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-074: los documentos vigentes de la empresa, para todas las personas con vínculo; se buscan por título.</summary>
    [HttpGet("vigentes")]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentoVigenteResumenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Vigentes(
        [FromQuery] FiltrosVigentesDto filtros, [FromServices] ListarVigentesServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(filtros, cancellationToken)).AHttp(this);

    /// <summary>HU-074: la versión vigente de un documento. Sin versión vigente (solo un borrador), 404.</summary>
    [HttpGet("vigentes/{id:guid}")]
    [ProducesResponseType(typeof(DocumentoVigenteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Vigente(Guid id, [FromServices] ObtenerVigenteServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);
}
