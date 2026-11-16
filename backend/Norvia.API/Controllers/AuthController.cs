using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Auth.CerrarSesion;
using Norvia.Application.Features.Auth.CerrarSesion.DTOs;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.EstablecerContrasena;
using Norvia.Application.Features.Auth.EstablecerContrasena.DTOs;
using Norvia.Application.Features.Auth.LoginEmpresa;
using Norvia.Application.Features.Auth.LoginEmpresa.DTOs;
using Norvia.Application.Features.Auth.LoginPlataforma;
using Norvia.Application.Features.Auth.LoginPlataforma.DTOs;
using Norvia.Application.Features.Auth.RefrescarSesion;
using Norvia.Application.Features.Auth.RefrescarSesion.DTOs;

namespace Norvia.API.Controllers;

/// <summary>Módulo Auth. Cada acción usa solo el servicio de su caso de uso.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login/empresa")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginEmpresa(
        LoginEmpresaRequestDto request, [FromServices] LoginEmpresaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [AllowAnonymous]
    [HttpPost("login/plataforma")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginPlataforma(
        LoginPlataformaRequestDto request, [FromServices] LoginPlataformaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refrescar(
        RefrescarSesionRequestDto request, [FromServices] RefrescarSesionServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarSesion(
        CerrarSesionRequestDto request, [FromServices] CerrarSesionServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// Cambia la contraseña de la sesión (HU-114; también el primer ingreso con contraseña temporal).
    /// Requiere sesión: no es anónima. Cierra todas las sesiones del Acceso.
    /// </summary>
    [HttpPost("contrasena/establecer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EstablecerContrasena(
        EstablecerContrasenaRequestDto request, [FromServices] EstablecerContrasenaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);
}

//prueba hu-095
[ApiController]
[Route("api/prueba")]
public sealed class PruebaController : ControllerBase
{
    [HttpGet("plataforma")] public IActionResult Plataforma() => Ok("pasó");
    [HttpGet("empresa")] public IActionResult Empresa() => Ok("pasó");
    [HttpGet("sin-permiso")] public IActionResult SinPermiso() => Ok("pasó");
}
