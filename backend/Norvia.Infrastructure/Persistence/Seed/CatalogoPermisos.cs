using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <param name="Roles">Códigos de rol: de empresa si el alcance es Empresa, de staff si es Plataforma.</param>
public sealed record PermisoDef(
    string Codigo, string Nombre, AlcancePermiso Alcance, string Metodo, string Ruta, string[] Roles);

public static class CatalogoPermisos
{
    private static PermisoDef Empresa(string codigo, string nombre, string metodo, string ruta, params string[] roles) =>
        new(codigo, nombre, AlcancePermiso.Empresa, metodo, ruta, roles);

    private static PermisoDef Plataforma(string codigo, string nombre, string metodo, string ruta, params string[] roles) =>
        new(codigo, nombre, AlcancePermiso.Plataforma, metodo, ruta, roles);

    public static readonly PermisoDef[] Todos =
    [
        // EJEMPLO de forma: reemplaza rutas y roles por los reales (ver 05-actores-roles.md).
        // El encuestado solo debe aparecer en estos tres.
        Empresa("encuesta.responder", "Responder encuesta", "POST", "api/encuestas/{id}/respuestas", "encuestado"),
        Empresa("notificacion.listar", "Ver notificaciones", "GET", "api/notificaciones", "encuestado"),
        Empresa("politica.vigentes", "Consultar políticas vigentes", "GET", "api/politicas/vigentes", "encuestado"),

        // TEMPORALES para probar el middleware (borrar junto con PruebaController).
        Plataforma("prueba.plataforma", "Prueba plataforma", "GET", "api/prueba/plataforma", "admin_plataforma"),
        Empresa("prueba.empresa", "Prueba empresa", "GET", "api/prueba/empresa", "admin_empresa"),
    ];
}