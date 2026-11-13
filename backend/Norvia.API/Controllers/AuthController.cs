using Microsoft.AspNetCore.Mvc;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.Interfaces;

namespace Norvia.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthServicio auth) : ControllerBase
{
    [HttpPost("login/empresa")]
    public async Task<IActionResult> LoginEmpresa([FromBody] LoginEmpresaRequest request, CancellationToken cancellationToken)
    {
        var r = await auth.LoginEmpresaAsync(request, cancellationToken);
        return r.Ok ? Ok(r.Tokens) : Unauthorized();
    }

    [HttpPost("login/plataforma")]
    public async Task<IActionResult> LoginPlataforma([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var r = await auth.LoginPlataformaAsync(request, cancellationToken);
        return r.Ok ? Ok(r.Tokens) : Unauthorized();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var r = await auth.RefreshAsync(request, cancellationToken);
        return r.Ok ? Ok(r.Tokens) : Unauthorized();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(request, cancellationToken);
        return NoContent();
    }
}
