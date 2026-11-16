using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.DTOs;

/// <summary>Acceso de una persona a la empresa (VinculoEmpresa), tal como lo ve el administrador.</summary>
public sealed record VinculoResponseDto(
    Guid Id,
    Guid PersonaId,
    string Email,
    string NombreCompleto,
    string RolCodigo,
    EstadoVinculo Estado,
    DateTime CreadoEn)
{
    /// <summary>Requiere Persona, Acceso y Rol cargados.</summary>
    public static VinculoResponseDto De(VinculoEmpresa v) =>
        new(v.Id, v.PersonaId, v.Acceso.Email, v.Persona.NombreCompleto, v.Rol.Codigo, v.Estado, v.CreadoEn);
}
