namespace Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;

public sealed record RegistrarEmpresaResponseDto(
    string IdEmpresa,
    string RazonSocial,
    string RTN,
    Guid VinculoId,
    string Email,
    bool CorreoEnviado);