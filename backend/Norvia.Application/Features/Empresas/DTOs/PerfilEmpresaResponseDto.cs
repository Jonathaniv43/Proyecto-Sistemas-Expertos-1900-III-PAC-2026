using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.DTOs;

/// <summary>Perfil de la empresa de la sesión (HU-026). Compartido por GET y PATCH.</summary>
public sealed record PerfilEmpresaResponseDto(
    Guid EmpresaId,
    string RazonSocial,
    string Rtn,
    string? Direccion,
    string? ActividadEconomica,
    short? RubroId,
    string? RubroNombre,
    int? TrabajadoresPermanentes,
    DateOnly? FechaInicioOperaciones,
    EstadoEmpresa Estado,
    bool PerfilCompleto)
{
    /// <summary>Requiere <see cref="Empresa.Rubro"/> cargado cuando hay <see cref="Empresa.RubroId"/>.</summary>
    public static PerfilEmpresaResponseDto De(Empresa e) =>
        new(
            e.Id,
            e.RazonSocial,
            e.RTN,
            e.Direccion,
            e.ActividadEconomica,
            e.RubroId,
            e.Rubro?.Nombre,
            e.TrabajadoresPermanentes,
            e.FechaInicioOperaciones,
            e.Estado,
            e.RubroId is not null && e.TrabajadoresPermanentes is not null);
}
