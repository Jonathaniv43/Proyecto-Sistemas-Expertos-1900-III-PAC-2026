using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Empresas.RegistrarEmpresa;

namespace Norvia.API.Controllers;

/// <summary>Módulo Empresas. Cada acción usa solo el servicio de su caso de uso.</summary>
[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Registrar(
        RegistrarEmpresaRequest request,
        [FromServices] RegistrarEmpresaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);
}