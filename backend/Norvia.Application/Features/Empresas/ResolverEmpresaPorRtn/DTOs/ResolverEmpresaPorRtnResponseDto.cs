namespace Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn.DTOs;

/// <summary>Datos mínimos para que el front arme el login con <c>idEmpresa</c> cifrado.</summary>
public sealed record ResolverEmpresaPorRtnResponseDto(
    string IdEmpresa,
    string RazonSocial,
    string Rtn);