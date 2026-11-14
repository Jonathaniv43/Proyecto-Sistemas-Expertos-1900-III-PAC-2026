using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Auth.CerrarSesion;
using Norvia.Application.Features.Auth.LoginEmpresa;
using Norvia.Application.Features.Auth.LoginPlataforma;
using Norvia.Application.Features.Auth.RefrescarSesion;

namespace Norvia.API.Controllers;

/// <summary>Módulo Auth. Cada acción usa solo el servicio de su caso de uso.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login/empresa")]
    public async Task<IActionResult> LoginEmpresa(
        LoginEmpresaRequest request, [FromServices] LoginEmpresaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [HttpPost("login/plataforma")]
    public async Task<IActionResult> LoginPlataforma(
        LoginPlataformaRequest request, [FromServices] LoginPlataformaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refrescar(
        RefrescarSesionRequest request, [FromServices] RefrescarSesionServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    [HttpPost("logout")]
    public async Task<IActionResult> CerrarSesion(
        CerrarSesionRequest request, [FromServices] CerrarSesionServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);
}
