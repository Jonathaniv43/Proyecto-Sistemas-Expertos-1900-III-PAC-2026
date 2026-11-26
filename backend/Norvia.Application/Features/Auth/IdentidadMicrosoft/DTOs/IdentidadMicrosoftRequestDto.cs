namespace Norvia.Application.Features.Auth.IdentidadMicrosoft.DTOs;

public sealed record IdentidadMicrosoftRequestDto
{
    /// <summary>Código de autorización que Microsoft devolvió al navegador.</summary>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>Verificador PKCE generado por el navegador.</summary>
    public string CodeVerifier { get; init; } = string.Empty;

    /// <summary>Debe estar en la lista permitida por configuración.</summary>
    public string RedirectUri { get; init; } = string.Empty;
}
