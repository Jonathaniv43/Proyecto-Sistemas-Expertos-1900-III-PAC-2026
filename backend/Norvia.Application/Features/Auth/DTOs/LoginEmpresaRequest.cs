namespace Norvia.Application.Features.Auth.DTOs;

public class LoginEmpresaRequest : LoginRequest
{
    /// <summary>EmpresaId cifrado por el backend al crear la empresa.</summary>
    public string IdEmpresa { get; set; } = string.Empty;
}
