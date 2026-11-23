using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.API.Seguridad;
using Norvia.Application.Features.Auth.CerrarSesion;
using Norvia.Application.Features.Auth.CerrarSesion.DTOs;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.ReestablecerContrasena;
using Norvia.Application.Features.Auth.ReestablecerContrasena.DTOs;
using Norvia.Application.Features.Auth.LoginEmpresa;
using Norvia.Application.Features.Auth.LoginEmpresa.DTOs;
using Norvia.Application.Features.Auth.LoginPlataforma;
using Norvia.Application.Features.Auth.LoginPlataforma.DTOs;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.DTOs;
using Norvia.Application.Features.Auth.RefrescarSesion;
using Norvia.Application.Features.Auth.RefrescarSesion.DTOs;
using Norvia.Application.Features.Auth.VerificarCuenta;
using Norvia.Application.Features.Auth.VerificarCuenta.DTOs;

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
        LoginEmpresaRequestDto request,
        [FromServices] LoginEmpresaServicio servicio,
        [FromServices] CookieSesion cookies,
        CancellationToken cancellationToken)
    {
        var resultado = await servicio.EjecutarAsync(request, cancellationToken);
        if (!resultado.EsExito)
            return resultado.AHttp(this);

        cookies.Establecer(Response, resultado.Valor);
        return Ok(resultado.Valor);
    }

    [AllowAnonymous]
    [HttpPost("login/plataforma")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginPlataforma(
        LoginPlataformaRequestDto request,
        [FromServices] LoginPlataformaServicio servicio,
        [FromServices] CookieSesion cookies,
        CancellationToken cancellationToken)
    {
        var resultado = await servicio.EjecutarAsync(request, cancellationToken);
        if (!resultado.EsExito)
            return resultado.AHttp(this);

        cookies.Establecer(Response, resultado.Valor);
        return Ok(resultado.Valor);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refrescar(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)]
        RefrescarSesionRequestDto? request,
        [FromServices] RefrescarSesionServicio servicio,
        [FromServices] CookieSesion cookies,
        CancellationToken cancellationToken)
    {
        var refresh = PrimeroNoVacio(request?.RefreshToken, cookies.LeerRefresh(Request));
        var resultado = await servicio.EjecutarAsync(
            new RefrescarSesionRequestDto { RefreshToken = refresh ?? string.Empty },
            cancellationToken);

        if (!resultado.EsExito)
        {
            cookies.Borrar(Response);
            return resultado.AHttp(this);
        }

        cookies.Establecer(Response, resultado.Valor);
        return Ok(resultado.Valor);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarSesion(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)]
        CerrarSesionRequestDto? request,
        [FromServices] CerrarSesionServicio servicio,
        [FromServices] CookieSesion cookies,
        CancellationToken cancellationToken)
    {
        var refresh = PrimeroNoVacio(request?.RefreshToken, cookies.LeerRefresh(Request));
        var resultado = await servicio.EjecutarAsync(
            new CerrarSesionRequestDto { RefreshToken = refresh ?? string.Empty },
            cancellationToken);

        cookies.Borrar(Response);
        return resultado.AHttp(this);
    }

    /// <summary>
    /// Cambia la contraseña de la sesión (HU-114; también el primer ingreso con contraseña temporal).
    /// Requiere sesión: no es anónima. Cierra todas las sesiones del Acceso.
    /// </summary>
    [HttpPost("contrasena/reestablecer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReestablecerContrasena(
        ReestablecerContrasenaRequestDto request, [FromServices] ReestablecerContrasenaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// Verifica la cuenta con correo + contraseña temporal del alta y fija la contraseña definitiva (HU-114).
    /// Anónimo: no hay JWT hasta que <c>Acceso.Verificado</c> sea true (RN-67).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("cuenta/verificar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerificarCuenta(
        VerificarCuentaRequestDto request, [FromServices] VerificarCuentaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// Reenvía instrucciones de verificación. Respuesta siempre genérica (202).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("cuenta/verificacion/reenvio")]
    [ProducesResponseType(typeof(ReenviarVerificacionCuentaResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReenviarVerificacionCuenta(
        ReenviarVerificacionCuentaRequestDto request, [FromServices] ReenviarVerificacionCuentaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpAceptado(this);

    private static string? PrimeroNoVacio(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) ? a : !string.IsNullOrWhiteSpace(b) ? b : null;
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
