namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

public sealed record RegistrarEmpresaRequest(
    string RazonSocial,
    string RTN,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string Email,
    string Password
);