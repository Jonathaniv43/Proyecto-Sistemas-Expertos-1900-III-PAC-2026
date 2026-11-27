using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa;
using Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa.DTOs;
using Norvia.Application.Features.Empresas.DTOs;
using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma;
using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;
using Norvia.Application.Features.Empresas.ListarRubrosRegistro;
using Norvia.Application.Features.Empresas.ListarRubrosRegistro.DTOs;
using Norvia.Application.Features.Empresas.ObtenerPerfilEmpresa;
using Norvia.Application.Features.Empresas.RegistrarEmpresa;
using Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;
using Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft;
using Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft.DTOs;
using Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn;
using Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn.DTOs;

namespace Norvia.API.Controllers;

/// <summary>Módulo Empresas. Cada acción usa solo el servicio de su caso de uso.</summary>
[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrarEmpresaResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Registrar(
        RegistrarEmpresaRequestDto request,
        [FromServices] RegistrarEmpresaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>
    /// D29, HU-119: registra la empresa con una cuenta de Microsoft. El correo sale del token de identidad
    /// (<c>POST /api/auth/microsoft/identidad</c>), no del cuerpo. Anónimo.
    /// </summary>
    [HttpPost("microsoft")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrarEmpresaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarConMicrosoft(
        RegistrarEmpresaMicrosoftRequestDto request,
        [FromServices] RegistrarEmpresaMicrosoftServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-017: rubros activos para el formulario de registro. Anónimo (aún no hay sesión).</summary>
    [HttpGet("rubros-registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<RubroRegistroResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRubrosRegistro(
        [FromServices] ListarRubrosRegistroServicio servicio,
        CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>
    /// Landing → login: resuelve <c>idEmpresa</c> cifrado por RTN (14 dígitos). Anónimo; solo empresas activas.
    /// </summary>
    [HttpGet("por-rtn")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResolverEmpresaPorRtnResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolverPorRtn(
        [FromQuery] ResolverEmpresaPorRtnRequestDto request,
        [FromServices] ResolverEmpresaPorRtnServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>HU-026: perfil de la empresa de la sesión.</summary>
    [HttpGet("perfil")]
    [ProducesResponseType(typeof(PerfilEmpresaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPerfil(
        [FromServices] ObtenerPerfilEmpresaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(cancellationToken)).AHttp(this);

    /// <summary>HU-026: actualiza uno o varios campos del perfil (sin RTN ni estado).</summary>
    [HttpPatch("perfil")]
    [ProducesResponseType(typeof(PerfilEmpresaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPerfil(
        ActualizarPerfilEmpresaRequestDto request,
        [FromServices] ActualizarPerfilEmpresaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>HU-024: empresas registradas en la plataforma y cantidad de vínculos vigentes (sin datos SST).</summary>
    [HttpGet("/api/plataforma/empresas")]
    [ProducesResponseType(typeof(IReadOnlyList<EmpresaResumenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarPlataforma(
        [FromQuery] FiltrosEmpresasPlataformaDto filtros,
        [FromServices] ListarEmpresasPlataformaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(filtros, cancellationToken)).AHttp(this);
}
