using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Eventos;

/// <summary>Reglas compartidas por los slices del módulo Eventos.</summary>
public static class Reglas
{
    /// <summary>Roles que pueden investigar un evento: los que trabajan con la gestión de SST, no los encuestados.</summary>
    public static readonly short[] RolesInvestigadores = [Rol.AdminEmpresaId, Rol.ResponsableSstId, Rol.ColaboradorId];

    /// <summary>Honduras no tiene horario de verano: su fecha es la de UTC−6.</summary>
    public static DateOnly FechaEnHonduras(DateTime utc) => DateOnly.FromDateTime(utc.AddHours(-6));
}
