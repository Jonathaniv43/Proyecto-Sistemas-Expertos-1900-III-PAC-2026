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
        // HU-031: responder (ampliado a todos los roles de empresa hasta convocatorias).
        Empresa("encuesta.pendientes", "Ver encuestas pendientes", "GET", "api/encuestas/pendientes",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),
        Empresa("encuesta.responder", "Responder encuesta", "POST", "api/encuestas/{id}/respuestas",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),

        // HU-117: la bandeja propia. Todos los roles, porque las políticas avisan a cada vínculo activo (RN-75);
        // que cada quien vea y marque solo las suyas (RN-79) lo valida la aplicación.
        Empresa("notificacion.listar", "Ver mis notificaciones", "GET", "api/notificaciones",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),
        Empresa("notificacion.no_leidas", "Ver cuántas notificaciones no he leído", "GET", "api/notificaciones/no-leidas",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),
        Empresa("notificacion.leer", "Marcar una notificación como leída", "PATCH", "api/notificaciones/{id}/lectura",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),

        // HU-070: generar políticas y procedimientos desde una plantilla (Responsable SST); el admin consulta.
        Empresa("documento.plantillas", "Ver las plantillas de documento del rubro", "GET", "api/documentos/plantillas", "responsable_sst"),
        Empresa("documento.generar", "Generar un documento desde una plantilla", "POST", "api/documentos/generacion", "responsable_sst"),
        Empresa("documento.listar", "Ver los documentos de la empresa", "GET", "api/documentos", "admin_empresa", "responsable_sst"),
        Empresa("documento.ver", "Ver un documento", "GET", "api/documentos/{id}", "admin_empresa", "responsable_sst"),
        // HU-071: solo el Responsable SST edita el borrador.
        Empresa("documento.editar", "Editar el borrador de un documento", "PUT", "api/documentos/{id}/borrador", "responsable_sst"),
        Empresa("documento.aprobar", "Aprobar el borrador de un documento", "POST", "api/documentos/{id}/aprobacion", "responsable_sst"),
        // HU-074: las versiones vigentes, para todas las personas de la empresa (también el encuestado, RN-74).
        Empresa("documento.vigentes", "Ver los documentos vigentes", "GET", "api/documentos/vigentes",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),
        Empresa("documento.vigente.ver", "Ver un documento vigente", "GET", "api/documentos/vigentes/{id}",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),

        // HU-033: peligros sugeridos por las respuestas.
        Empresa("encuesta.peligros_sugeridos", "Ver peligros sugeridos por la encuesta", "GET", "api/encuestas/{id}/peligros-sugeridos",
            "admin_empresa", "responsable_sst"),

        // HU-035: confirmar o descartar un peligro sugerido.
        Empresa("encuesta.peligro_sugerido.revisar", "Confirmar o descartar un peligro sugerido", "PATCH",
            "api/encuestas/{id}/peligros-sugeridos/{riesgoId}", "admin_empresa", "responsable_sst"),

        // HU-036: matriz de riesgos de la empresa.
        Empresa("riesgo.listar", "Ver los riesgos de la matriz", "GET", "api/riesgos", "admin_empresa", "responsable_sst"),
        Empresa("riesgo.leer", "Ver un riesgo", "GET", "api/riesgos/{id}", "admin_empresa", "responsable_sst"),
        Empresa("riesgo.peligros", "Ver el catálogo de peligros para agregar riesgos", "GET", "api/riesgos/peligros-disponibles",
            "admin_empresa", "responsable_sst"),
        Empresa("riesgo.crear", "Agregar un riesgo a la matriz", "POST", "api/riesgos", "admin_empresa", "responsable_sst"),
        // HU-038: mapa de la matriz.
        Empresa("riesgo.matriz", "Ver el mapa de la matriz de riesgos", "GET", "api/riesgos/matriz", "admin_empresa", "responsable_sst"),
        // HU-037: evaluar un riesgo.
        Empresa("riesgo.evaluar", "Evaluar un riesgo", "POST", "api/riesgos/{id}/evaluaciones", "admin_empresa", "responsable_sst"),
        // HU-042: plan de trabajo.
        Empresa("plan.propuesta", "Ver el plan de trabajo propuesto", "GET", "api/plan-trabajo/propuesta", "admin_empresa", "responsable_sst"),
        Empresa("plan.generar", "Confirmar el plan de trabajo", "POST", "api/plan-trabajo/generacion", "admin_empresa", "responsable_sst"),
        Empresa("tarea.listar", "Ver las tareas del plan", "GET", "api/plan-trabajo/tareas", "admin_empresa", "responsable_sst"),
        // HU-044: atender mis tareas. Quién cambia el estado lo decide además la aplicación (RN-24).
        Empresa("tarea.mias", "Ver mis tareas", "GET", "api/plan-trabajo/tareas/mias", "admin_empresa", "responsable_sst", "colaborador"),
        Empresa("tarea.estado", "Cambiar el estado de una tarea", "PATCH", "api/plan-trabajo/tareas/{id}/estado",
            "admin_empresa", "responsable_sst", "colaborador"),
        // HU-043: asignar tareas y crear tareas manuales.
        Empresa("tarea.crear", "Crear una tarea manual", "POST", "api/plan-trabajo/tareas", "admin_empresa", "responsable_sst"),
        Empresa("tarea.asignar", "Asignar responsable y fecha límite a una tarea", "PUT", "api/plan-trabajo/tareas/{id}/asignacion",
            "admin_empresa", "responsable_sst"),
        Empresa("tarea.responsables", "Ver a quién se le puede asignar una tarea", "GET", "api/plan-trabajo/responsables",
            "admin_empresa", "responsable_sst"),
        // HU-041: historial de evaluaciones.
        Empresa("riesgo.historial", "Ver el historial de evaluaciones de un riesgo", "GET", "api/riesgos/{id}/evaluaciones",
            "admin_empresa", "responsable_sst"),
        // HU-040: evaluación residual.
        Empresa("riesgo.evaluar_residual", "Evaluar el riesgo residual", "POST", "api/riesgos/{id}/evaluaciones-residual",
            "admin_empresa", "responsable_sst"),
        // HU-039: controles de un riesgo.
        Empresa("riesgo.control.listar", "Ver los controles de un riesgo", "GET", "api/riesgos/{id}/controles",
            "admin_empresa", "responsable_sst"),
        Empresa("riesgo.control.sugeridos", "Ver los controles sugeridos para un riesgo", "GET", "api/riesgos/{id}/controles/sugeridos",
            "admin_empresa", "responsable_sst"),
        Empresa("riesgo.control.crear", "Agregar un control a un riesgo", "POST", "api/riesgos/{id}/controles",
            "admin_empresa", "responsable_sst"),

        // HU-032: reporte anónimo.
        Empresa("encuesta.reporte", "Ver reporte anónimo de encuesta", "GET", "api/encuestas/{id}/reporte",
            "admin_empresa", "responsable_sst"),

        // HU-114: cambiar la contraseña propia (incluye el primer ingreso con contraseña temporal).
        Empresa("auth.contrasena.reestablecer", "Cambiar mi contraseña", "POST", "api/auth/contrasena/reestablecer",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),

        // HU-020: agregar personas a la empresa.
        Empresa("vinculo.crear", "Agregar persona a la empresa", "POST", "api/vinculos", "admin_empresa"),

        // HU-115: cuentas de encuestados (individual y por lote).
        Empresa("vinculo.encuestado.crear", "Agregar encuestado", "POST", "api/vinculos/encuestados", "admin_empresa"),
        Empresa("vinculo.encuestado.lote", "Agregar encuestados por lote", "POST", "api/vinculos/encuestados/lote", "admin_empresa"),

        // HU-021: administrar el acceso de las personas.
        Empresa("vinculo.listar", "Listar personas con acceso", "GET", "api/vinculos", "admin_empresa"),
        Empresa("vinculo.leer", "Ver el acceso de una persona", "GET", "api/vinculos/{id}", "admin_empresa"),
        Empresa("vinculo.actualizar", "Cambiar rol o estado de un acceso", "PATCH", "api/vinculos/{id}", "admin_empresa"),

        // HU-026: perfil de la empresa de la sesión.
        Empresa("empresa.perfil.leer", "Ver perfil de la empresa", "GET", "api/empresas/perfil",
            "admin_empresa", "responsable_sst"),
        Empresa("empresa.perfil.actualizar", "Actualizar perfil de la empresa", "PATCH", "api/empresas/perfil",
            "admin_empresa"),

        // HU-029: elegir categorías de riesgo para una encuesta.
        Empresa("encuesta.categoria.listar", "Ver categorías de riesgo disponibles", "GET", "api/encuestas/categorias-disponibles",
            "admin_empresa", "responsable_sst"),

        // Tipos de respuesta (instrucción UI para el editor / encuestado).
        Empresa("encuesta.tipo.listar", "Ver tipos de respuesta de pregunta", "GET", "api/encuestas/tipos-respuesta",
            "admin_empresa", "responsable_sst"),

        // HU-030: crear, editar, publicar, cerrar y consultar encuestas.
        Empresa("encuesta.listar", "Listar encuestas", "GET", "api/encuestas", "admin_empresa", "responsable_sst"),
        Empresa("encuesta.crear", "Crear encuesta", "POST", "api/encuestas", "admin_empresa", "responsable_sst"),
        Empresa("encuesta.leer", "Ver detalle de encuesta", "GET", "api/encuestas/{id}", "admin_empresa", "responsable_sst"),
        Empresa("encuesta.editar", "Editar borrador de encuesta", "PUT", "api/encuestas/{id}/borrador", "admin_empresa", "responsable_sst"),
        Empresa("encuesta.publicar", "Publicar versión de encuesta", "POST", "api/encuestas/{id}/versiones/{versionId}/publicacion",
            "admin_empresa", "responsable_sst"),
        Empresa("encuesta.cerrar", "Cerrar encuesta", "POST", "api/encuestas/{id}/cierre", "admin_empresa", "responsable_sst"),
        // HU-034: historial y comparación de encuestas y versiones (RN-12, D8).
        Empresa("encuesta.historial", "Consultar encuestas anteriores", "GET", "api/encuestas/historial",
            "admin_empresa", "responsable_sst"),
        Empresa("encuesta.comparar", "Comparar resultados entre encuestas o versiones", "GET", "api/encuestas/comparacion",
            "admin_empresa", "responsable_sst"),

        // HU-047: reporte anónimo (RN-27, RN-74) y consulta básica de eventos (M6).
        // Pendiente HU-100: evento.evidencias (POST api/eventos/{id}/evidencias).
        // Pendiente HU-049/050/052: investigación, descarte, medidas correctivas y estadísticas.
        Empresa("evento.reportar", "Reportar evento de forma anónima", "POST", "api/eventos",
            "admin_empresa", "responsable_sst", "colaborador", "encuestado"),
        Empresa("evento.listar", "Listar eventos reportados", "GET", "api/eventos", "admin_empresa", "responsable_sst"),
        Empresa("evento.ver", "Ver detalle de evento", "GET", "api/eventos/{id}", "admin_empresa", "responsable_sst"),
        // HU-049: investigar, cerrar y descartar eventos (Responsable SST).
        Empresa("evento.investigar", "Registrar la investigación de un evento", "PUT", "api/eventos/{id}/investigacion", "responsable_sst"),
        Empresa("evento.cerrar", "Cerrar un evento", "POST", "api/eventos/{id}/cierre", "responsable_sst"),
        Empresa("evento.descartar", "Descartar un evento", "PATCH", "api/eventos/{id}/descarte", "responsable_sst"),
        // HU-052: estadísticas de eventos, solo cifras (art. 47.e).
        Empresa("evento.estadisticas", "Ver las estadísticas de eventos", "GET", "api/eventos/estadisticas", "admin_empresa", "responsable_sst"),
        // HU-051: los riesgos de la matriz que el evento reveló o hizo reevaluar.
        Empresa("evento.riesgos", "Ver los riesgos enlazados con un evento", "GET", "api/eventos/{id}/riesgos", "admin_empresa", "responsable_sst"),
        // HU-050: medidas correctivas, que son tareas del plan con origen en el evento.
        Empresa("evento.medida.listar", "Ver las medidas correctivas de un evento", "GET", "api/eventos/{id}/medidas-correctivas",
            "admin_empresa", "responsable_sst"),
        Empresa("evento.medida.crear", "Agregar una medida correctiva a un evento", "POST", "api/eventos/{id}/medidas-correctivas",
            "responsable_sst"),

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
        // HU-023: plantillas de tarea de cada peligro (controles sugeridos y tareas modelo).
        Plataforma("catalogo.plantilla_tarea.listar", "Listar plantillas de tarea", "GET", "api/plantillas-tarea", "admin_plataforma", "soporte_plataforma"),
        Plataforma("catalogo.plantilla_tarea.crear", "Crear plantilla de tarea", "POST", "api/plantillas-tarea", "admin_plataforma"),
        Plataforma("catalogo.plantilla_tarea.actualizar", "Editar plantilla de tarea", "PATCH", "api/plantillas-tarea/{id}", "admin_plataforma"),
        Plataforma("catalogo.plantilla_tarea.estado", "Activar o desactivar plantilla de tarea", "PATCH", "api/plantillas-tarea/{id}/estado", "admin_plataforma"),

        // TEMPORALES para probar el middleware (borrar junto con PruebaController).
        Plataforma("prueba.plataforma", "Prueba plataforma", "GET", "api/prueba/plataforma", "admin_plataforma"),
        Empresa("prueba.empresa", "Prueba empresa", "GET", "api/prueba/empresa", "admin_empresa"),
    ];
}