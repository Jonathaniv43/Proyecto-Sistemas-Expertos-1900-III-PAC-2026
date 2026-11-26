namespace Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft.DTOs;

/// <summary>
/// Igual que el registro con contraseña, pero sin correo ni contraseña: el correo sale del token de identidad
/// firmado (RN-84). El documento es opcional: se completa después en el perfil (HU-026).
/// </summary>
public sealed record RegistrarEmpresaMicrosoftRequestDto(
    string TokenIdentidad,
    string RazonSocial,
    string RTN,
    string RubroCodigo,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? TipoDocumento,
    string? NumeroDocumento);
