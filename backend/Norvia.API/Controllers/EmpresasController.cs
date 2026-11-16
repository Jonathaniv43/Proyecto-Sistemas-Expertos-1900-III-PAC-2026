using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Empresas.RegistrarEmpresa;
using Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;

namespace Norvia.API.Controllers;

/// <summary>Módulo Empresas. Cada acción usa solo el servicio de su caso de uso.</summary>
[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrarEmpresaResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Registrar(
        RegistrarEmpresaRequestDto request,
        [FromServices] RegistrarEmpresaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);
}