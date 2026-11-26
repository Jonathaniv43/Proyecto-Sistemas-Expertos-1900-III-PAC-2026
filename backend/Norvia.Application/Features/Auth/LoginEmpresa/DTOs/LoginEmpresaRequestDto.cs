namespace Norvia.Application.Features.Auth.LoginEmpresa.DTOs;

public sealed record LoginEmpresaRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    /// <summary>EmpresaId cifrado por el backend al crear la empresa.</summary>
    public string IdEmpresa { get; init; } = string.Empty;
}
