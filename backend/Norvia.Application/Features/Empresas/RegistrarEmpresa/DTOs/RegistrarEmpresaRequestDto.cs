namespace Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;

public sealed record RegistrarEmpresaRequestDto(
    string RazonSocial,
    string RTN,
    string RubroCodigo,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string TipoDocumento,
    string NumeroDocumento,
    string Email);