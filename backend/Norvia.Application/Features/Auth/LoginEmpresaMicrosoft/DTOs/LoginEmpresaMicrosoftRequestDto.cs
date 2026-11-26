namespace Norvia.Application.Features.Auth.LoginEmpresaMicrosoft.DTOs;

public sealed record LoginEmpresaMicrosoftRequestDto
{
    public string TokenIdentidad { get; init; } = string.Empty;

    /// <summary>EmpresaId cifrado, como en el login con contraseña.</summary>
    public string IdEmpresa { get; init; } = string.Empty;
}
