namespace Norvia.Application.Features.Auth.VerificarCodigoMicrosoft.DTOs;

public sealed record VerificarCodigoMicrosoftRequestDto
{
    public string TokenIdentidad { get; init; } = string.Empty;

    /// <summary>Los 6 dígitos que llegaron al correo.</summary>
    public string Codigo { get; init; } = string.Empty;
}

/// <summary>Nuevo token de identidad, ya con el correo confirmado: es el que se manda al registrar la empresa.</summary>
public sealed record VerificarCodigoMicrosoftResponseDto(string TokenIdentidad);
