using Norvia.Domain.Enums;

namespace Norvia.Application.Common.Seguridad;

/// <summary>Datos que van como claims en el JWT de acceso.</summary>
public sealed record SesionToken(
    Guid AccesoId,
    TipoSesion Tipo,
    Guid? VinculoEmpresaId,
    Guid? EmpresaId,
    string? RolCodigo,
    /// <summary>Correo del Acceso: la web lo muestra (p. ej. a dónde se envía el enlace de HU-022).</summary>
    string Email,
    /// <summary>Si la cuenta tiene contraseña (una creada con Microsoft puede no tenerla, D29). Solo para la interfaz.</summary>
    bool TienePassword);
