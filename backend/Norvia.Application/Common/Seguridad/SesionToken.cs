using Norvia.Domain.Enums;

namespace Norvia.Application.Common.Seguridad;

/// <summary>Datos que van como claims en el JWT de acceso.</summary>
public sealed record SesionToken(
    Guid AccesoId,
    TipoSesion Tipo,
    Guid? VinculoEmpresaId,
    Guid? EmpresaId,
    string? RolCodigo);
