using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;

/// <summary>
/// HU-024: datos generales de uso de la plataforma por empresa (sin datos SST ni detalle de personas).
/// </summary>
public sealed record EmpresaResumenResponseDto(
    Guid EmpresaId,
    string RazonSocial,
    string? RubroNombre,
    EstadoEmpresa Estado,
    DateTime RegistradaEn,
    int VinculosActivos);
