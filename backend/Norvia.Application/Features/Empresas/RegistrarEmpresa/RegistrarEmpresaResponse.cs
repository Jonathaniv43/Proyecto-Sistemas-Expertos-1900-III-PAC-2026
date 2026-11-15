namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

public sealed record RegistrarEmpresaResponse(
    string IdEmpresa,
    string RazonSocial,
    string RTN,
    Guid VinculoId,
    string Email
);