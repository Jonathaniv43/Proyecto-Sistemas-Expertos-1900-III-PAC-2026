using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Auth.CerrarSesion;
using Norvia.Application.Features.Auth.CerrarSesion.DTOs;
using Norvia.Application.Features.Auth.DTOs;
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
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login/empresa")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginEmpresa(
        LoginEmpresaRequestDto request, [FromServices] LoginEmpresaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [HttpPost("login/plataforma")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginPlataforma(
        LoginPlataformaRequestDto request, [FromServices] LoginPlataformaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refrescar(
        RefrescarSesionRequestDto request, [FromServices] RefrescarSesionServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarSesion(
        CerrarSesionRequestDto request, [FromServices] CerrarSesionServicio servicio, CancellationToken cancellationToken) =>
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
