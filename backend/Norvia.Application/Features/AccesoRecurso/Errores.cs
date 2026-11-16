using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.AccesoRecurso;

/// <summary>Errores de autorización por permiso (middleware AccesoRecurso).</summary>
public static class Errores
{
    public static readonly Error NoAutenticado =
        new("auth.no_autenticado", "Debe iniciar sesión.", TipoError.NoAutorizado);

    /// <summary>Ruta/método no están en el catálogo (o permiso inactivo). HTTP 404 + mensaje en ProblemDetails.</summary>
    public static readonly Error PermisoNoDisponible =
        new("permiso.no_disponible", "La ruta no está registrada. Contacte con un administrador.", TipoError.NoEncontrado);

    /// <summary>Permiso existe pero el rol/alcance de la sesión no lo tiene. HTTP 403.</summary>
    public static readonly Error PermisoProhibido =
        new("permiso.prohibido", "No tiene permiso para realizar esta acción.", TipoError.Prohibido);
}
