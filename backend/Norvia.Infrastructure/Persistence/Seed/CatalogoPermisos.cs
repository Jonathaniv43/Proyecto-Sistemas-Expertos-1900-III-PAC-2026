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
        // El encuestado solo debe aparecer en estos tres y en el cambio de contraseña (HU-114).
        Empresa("encuesta.responder", "Responder encuesta", "POST", "api/encuestas/{id}/respuestas", "encuestado"),
        Empresa("notificacion.listar", "Ver notificaciones", "GET", "api/notificaciones", "encuestado"),
        Empresa("politica.vigentes", "Consultar políticas vigentes", "GET", "api/politicas/vigentes", "encuestado"),

        // HU-114: cambiar la contraseña propia (incluye el primer ingreso con contraseña temporal).
        Empresa("auth.contrasena.establecer", "Cambiar mi contraseña", "POST", "api/auth/contrasena/establecer",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),

        // HU-020: agregar personas a la empresa.
        Empresa("vinculo.crear", "Agregar persona a la empresa", "POST", "api/vinculos", "admin_empresa"),

        // HU-021: administrar el acceso de las personas.
        Empresa("vinculo.listar", "Listar personas con acceso", "GET", "api/vinculos", "admin_empresa"),
        Empresa("vinculo.leer", "Ver el acceso de una persona", "GET", "api/vinculos/{id}", "admin_empresa"),
        Empresa("vinculo.actualizar", "Cambiar rol o estado de un acceso", "PATCH", "api/vinculos/{id}", "admin_empresa"),

        // HU-023: catálogos globales (alcance plataforma). Soporte solo consulta; el administrador los mantiene.
        Plataforma("catalogo.rubro.listar", "Listar rubros", "GET", "api/rubros", "admin_plataforma", "soporte_plataforma"),
        Plataforma("catalogo.rubro.crear", "Crear rubro", "POST", "api/rubros", "admin_plataforma"),
        Plataforma("catalogo.rubro.actualizar", "Editar rubro", "PATCH", "api/rubros/{id}", "admin_plataforma"),
        Plataforma("catalogo.rubro.estado", "Activar o desactivar rubro", "PATCH", "api/rubros/{id}/estado", "admin_plataforma"),
        Plataforma("catalogo.categoria.listar", "Listar categorías de riesgo", "GET", "api/categorias-riesgo", "admin_plataforma", "soporte_plataforma"),
        Plataforma("catalogo.categoria.crear", "Crear categoría de riesgo", "POST", "api/categorias-riesgo", "admin_plataforma"),
        Plataforma("catalogo.categoria.actualizar", "Editar categoría de riesgo", "PATCH", "api/categorias-riesgo/{id}", "admin_plataforma"),
        Plataforma("catalogo.categoria.estado", "Activar o desactivar categoría de riesgo", "PATCH", "api/categorias-riesgo/{id}/estado", "admin_plataforma"),
        Plataforma("catalogo.peligro.listar", "Listar peligros", "GET", "api/peligros", "admin_plataforma", "soporte_plataforma"),
        Plataforma("catalogo.peligro.crear", "Crear peligro", "POST", "api/peligros", "admin_plataforma"),
        Plataforma("catalogo.peligro.actualizar", "Editar peligro", "PATCH", "api/peligros/{id}", "admin_plataforma"),
        Plataforma("catalogo.peligro.estado", "Activar o desactivar peligro", "PATCH", "api/peligros/{id}/estado", "admin_plataforma"),
        Plataforma("catalogo.pregunta.listar", "Listar preguntas exploratorias", "GET", "api/preguntas", "admin_plataforma", "soporte_plataforma"),
        Plataforma("catalogo.pregunta.crear", "Crear pregunta exploratoria", "POST", "api/preguntas", "admin_plataforma"),
        Plataforma("catalogo.pregunta.actualizar", "Editar pregunta exploratoria", "PATCH", "api/preguntas/{id}", "admin_plataforma"),
        Plataforma("catalogo.pregunta.estado", "Activar o desactivar pregunta exploratoria", "PATCH", "api/preguntas/{id}/estado", "admin_plataforma"),
        Plataforma("catalogo.plantilla.listar", "Listar plantillas de documento", "GET", "api/plantillas", "admin_plataforma", "soporte_plataforma"),
        Plataforma("catalogo.plantilla.crear", "Crear plantilla de documento", "POST", "api/plantillas", "admin_plataforma"),
        Plataforma("catalogo.plantilla.actualizar", "Editar plantilla de documento", "PATCH", "api/plantillas/{id}", "admin_plataforma"),
        Plataforma("catalogo.plantilla.estado", "Activar o desactivar plantilla de documento", "PATCH", "api/plantillas/{id}/estado", "admin_plataforma"),

        // TEMPORALES para probar el middleware (borrar junto con PruebaController).
        Plataforma("prueba.plataforma", "Prueba plataforma", "GET", "api/prueba/plataforma", "admin_plataforma"),
        Empresa("prueba.empresa", "Prueba empresa", "GET", "api/prueba/empresa", "admin_empresa"),
    ];
}