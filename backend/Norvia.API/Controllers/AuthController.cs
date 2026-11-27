using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Auth.CerrarSesion;
using Norvia.Application.Features.Auth.CerrarSesion.DTOs;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.ConfirmarCambioContrasena;
using Norvia.Application.Features.Auth.ConfirmarCambioContrasena.DTOs;
using Norvia.Application.Features.Auth.ReestablecerContrasena;
using Norvia.Application.Features.Auth.SolicitarCambioContrasena;
using Norvia.Application.Features.Auth.SolicitarCambioContrasena.DTOs;
using Norvia.Application.Features.Auth.ReestablecerContrasena.DTOs;
using Norvia.Application.Features.Auth.EnviarCodigoMicrosoft;
using Norvia.Application.Features.Auth.EnviarCodigoMicrosoft.DTOs;
using Norvia.Application.Features.Auth.IdentidadMicrosoft;
using Norvia.Application.Features.Auth.IdentidadMicrosoft.DTOs;
using Norvia.Application.Features.Auth.LoginEmpresa;
using Norvia.Application.Features.Auth.LoginEmpresaMicrosoft;
using Norvia.Application.Features.Auth.LoginEmpresaMicrosoft.DTOs;
using Norvia.Application.Features.Auth.VerificarCodigoMicrosoft;
using Norvia.Application.Features.Auth.VerificarCodigoMicrosoft.DTOs;
using Norvia.Application.Features.Auth.VincularMicrosoft;
using Norvia.Application.Features.Auth.VincularMicrosoft.DTOs;
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
        LoginEmpresaRequestDto request, [FromServices] LoginEmpresaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// D29, HU-119/120: canjea el código de Microsoft (OAuth con PKCE) y devuelve un token de identidad de corta
    /// vida, el correo verificado, los nombres para prellenar y el estado de la cuenta. Anónimo.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("microsoft/identidad")]
    [ProducesResponseType(typeof(IdentidadMicrosoftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IdentidadMicrosoft(
        IdentidadMicrosoftRequestDto request, [FromServices] IdentidadMicrosoftServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// D29, RN-86: envía un código de 6 dígitos al correo de la cuenta de Microsoft que aún no tiene cuenta en
    /// Norvia, para probar que el correo es de quien se registra. Máximo 3 envíos cada 10 minutos. Anónimo.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("microsoft/codigo")]
    [ProducesResponseType(typeof(EnviarCodigoMicrosoftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EnviarCodigoMicrosoft(
        EnviarCodigoMicrosoftRequestDto request, [FromServices] EnviarCodigoMicrosoftServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// D29, RN-86: comprueba el código y devuelve el token de identidad con el correo confirmado. Máximo 5
    /// intentos cada 10 minutos. Anónimo.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("microsoft/codigo/verificar")]
    [ProducesResponseType(typeof(VerificarCodigoMicrosoftResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerificarCodigoMicrosoft(
        VerificarCodigoMicrosoftRequestDto request, [FromServices] VerificarCodigoMicrosoftServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>HU-120: inicia sesión en una empresa con una cuenta de Microsoft ya vinculada. Anónimo.</summary>
    [AllowAnonymous]
    [HttpPost("login/empresa/microsoft")]
    [ProducesResponseType(typeof(SesionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginEmpresaMicrosoft(
        LoginEmpresaMicrosoftRequestDto request, [FromServices] LoginEmpresaMicrosoftServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>HU-120, RN-83: vincula Microsoft a una cuenta con contraseña, confirmando esa contraseña. Anónimo.</summary>
    [AllowAnonymous]
    [HttpPost("microsoft/vincular")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VincularMicrosoft(
        VincularMicrosoftRequestDto request, [FromServices] VincularMicrosoftServicio servicio,
        CancellationToken cancellationToken) =>
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
    [HttpPost("contrasena/reestablecer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReestablecerContrasena(
        ReestablecerContrasenaRequestDto request, [FromServices] ReestablecerContrasenaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// HU-022: envía al correo de la cuenta un enlace para cambiar la contraseña. Respuesta siempre genérica (202).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("contrasena/solicitud")]
    [ProducesResponseType(typeof(SolicitarCambioContrasenaResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SolicitarCambioContrasena(
        SolicitarCambioContrasenaRequestDto request, [FromServices] SolicitarCambioContrasenaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpAceptado(this);

    /// <summary>
    /// HU-022: cambia la contraseña con el token del enlace. Vale en todas las empresas de la cuenta y
    /// cierra todas sus sesiones. 410 si el enlace venció, ya se usó o fue alterado.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("contrasena/confirmacion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    public async Task<IActionResult> ConfirmarCambioContrasena(
        ConfirmarCambioContrasenaRequestDto request, [FromServices] ConfirmarCambioContrasenaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>
    /// Verifica la cuenta con el código de 6 dígitos del alta y fija la contraseña definitiva (HU-114, RN-86).
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
