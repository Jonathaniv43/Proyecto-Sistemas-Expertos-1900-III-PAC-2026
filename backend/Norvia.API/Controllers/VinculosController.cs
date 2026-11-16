using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Vinculos.ActualizarVinculo;
using Norvia.Application.Features.Vinculos.ActualizarVinculo.DTOs;
using Norvia.Application.Features.Vinculos.CrearVinculo;
using Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;
using Norvia.Application.Features.Vinculos.DTOs;
using Norvia.Application.Features.Vinculos.ListarVinculos;
using Norvia.Application.Features.Vinculos.ObtenerVinculo;
using Norvia.Domain.Enums;

namespace Norvia.API.Controllers;

/// <summary>Módulo Vinculos: acceso de las personas a la empresa de la sesión.</summary>
[ApiController]
[Route("api/vinculos")]
public sealed class VinculosController : ControllerBase
{
    /// <summary>HU-021: personas con acceso a la empresa. Filtros opcionales por estado y rol.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VinculoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] EstadoVinculo? estado, [FromQuery] string? rolCodigo,
        [FromServices] ListarVinculosServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(estado, rolCodigo, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VinculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(
        Guid id, [FromServices] ObtenerVinculoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-020: agrega una persona a la empresa con un rol, creando o reutilizando su cuenta.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CrearVinculoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        CrearVinculoRequestDto request, [FromServices] CrearVinculoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-021: cambia el rol o el estado (con motivo e historial).</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(VinculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        Guid id, ActualizarVinculoRequestDto request, [FromServices] ActualizarVinculoServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);
}
