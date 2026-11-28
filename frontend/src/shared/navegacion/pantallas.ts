import { matchPath } from 'react-router-dom'

/**
 * Lista única de pantallas (design-system.md § Layouts y navegación): de aquí salen el título de la pestaña y el texto
 * de "Volver" cuando se regresa a cada pantalla. Una pantalla nueva se agrega aquí además de en app/router.tsx.
 *
 * - `nombre`: título de la pestaña ("{nombre} · Norvia"). En las pantallas del menú es el nombre del ítem.
 * - `volver`: "Volver a {destino}" al regresar a esta pantalla. Sin él, "Volver" usa la pantalla padre fija
 *   (p. ej. no se vuelve a un formulario ya guardado).
 */
export interface Pantalla {
  patron: string
  nombre: string
  volver?: string
}

export const PANTALLAS: readonly Pantalla[] = [
  // Públicas
  { patron: '/', nombre: 'Norvia', volver: 'Volver a la página principal' },
  // Sin `volver`: el login tiene dos pasos y su texto lo decide historialVolver ("Volver a buscar tu empresa").
  { patron: '/login', nombre: 'Iniciar sesión' },
  { patron: '/plataforma', nombre: 'Acceso del equipo de Norvia', volver: 'Volver al acceso del equipo' },
  { patron: '/registro', nombre: 'Registra tu empresa', volver: 'Volver al registro' },
  { patron: '/verificar-cuenta', nombre: 'Activa tu cuenta' },
  { patron: '/recuperar-contrasena', nombre: '¿Olvidaste tu contraseña?' },
  { patron: '/restablecer-contrasena', nombre: 'Elige tu contraseña nueva' },
  { patron: '/auth/microsoft/callback', nombre: 'Entrando con Microsoft' },

  // Con sesión: comunes
  { patron: '/inicio', nombre: 'Inicio', volver: 'Volver al inicio' },
  { patron: '/cuenta/contrasena', nombre: 'Cambiar mi contraseña', volver: 'Volver a cambiar tu contraseña' },
  { patron: '/notificaciones', nombre: 'Notificaciones', volver: 'Volver a notificaciones' },
  { patron: '/mis-encuestas', nombre: 'Mis encuestas', volver: 'Volver a mis encuestas' },
  { patron: '/mis-encuestas/:id', nombre: 'Responder encuesta', volver: 'Volver a la encuesta' },
  { patron: '/mis-tareas', nombre: 'Mis tareas', volver: 'Volver a mis tareas' },
  { patron: '/documentos-vigentes', nombre: 'Políticas y procedimientos', volver: 'Volver a políticas y procedimientos' },
  { patron: '/documentos-vigentes/:id', nombre: 'Documento vigente', volver: 'Volver al documento' },
  { patron: '/eventos/reportar', nombre: 'Reportar un evento', volver: 'Volver a reportar un evento' },

  // Gestión SST
  { patron: '/encuestas', nombre: 'Encuestas', volver: 'Volver a encuestas' },
  { patron: '/encuestas/nueva', nombre: 'Nueva encuesta', volver: 'Volver a nueva encuesta' },
  { patron: '/encuestas/historial', nombre: 'Encuestas anteriores', volver: 'Volver a encuestas anteriores' },
  { patron: '/encuestas/comparar', nombre: 'Comparar resultados', volver: 'Volver a comparar resultados' },
  { patron: '/encuestas/:id', nombre: 'Encuesta', volver: 'Volver a la encuesta' },
  { patron: '/encuestas/:id/resultados', nombre: 'Resultados de la encuesta', volver: 'Volver a los resultados' },
  { patron: '/riesgos', nombre: 'Matriz de riesgos', volver: 'Volver a la matriz de riesgos' },
  { patron: '/riesgos/nuevo', nombre: 'Agregar riesgo', volver: 'Volver a agregar riesgo' },
  { patron: '/riesgos/imprimir', nombre: 'Imprimir la matriz de riesgos', volver: 'Volver a imprimir la matriz' },
  { patron: '/riesgos/:id', nombre: 'Riesgo', volver: 'Volver al riesgo' },
  { patron: '/plan-de-trabajo', nombre: 'Plan de trabajo', volver: 'Volver al plan de trabajo' },
  { patron: '/plan-de-trabajo/nueva', nombre: 'Nueva tarea' },
  { patron: '/eventos', nombre: 'Eventos', volver: 'Volver a eventos' },
  { patron: '/eventos/estadisticas', nombre: 'Estadísticas de eventos', volver: 'Volver a las estadísticas de eventos' },
  { patron: '/eventos/:id', nombre: 'Evento', volver: 'Volver al evento' },
  { patron: '/documentos', nombre: 'Documentos', volver: 'Volver a documentos' },
  { patron: '/documentos/generar', nombre: 'Generar documento' },
  { patron: '/documentos/:id', nombre: 'Documento', volver: 'Volver al documento' },
  // Tras guardar el borrador se vuelve al documento y "Volver" lleva a la lista, no al editor (§ Editor de documento).
  { patron: '/documentos/:id/editar', nombre: 'Editar borrador' },
  { patron: '/documentos/:id/versiones/:versionId', nombre: 'Versión del documento', volver: 'Volver a la versión' },

  // Empresa
  { patron: '/personas', nombre: 'Personas', volver: 'Volver a personas' },
  { patron: '/personas/nueva', nombre: 'Agregar una persona', volver: 'Volver a agregar una persona' },
  { patron: '/personas/encuestados', nombre: 'Agregar encuestados', volver: 'Volver a agregar encuestados' },
  { patron: '/empresa', nombre: 'Perfil de la empresa', volver: 'Volver al perfil de la empresa' },
  { patron: '/auditoria', nombre: 'Auditoría', volver: 'Volver a auditoría' },

  // Plataforma
  { patron: '/empresas', nombre: 'Empresas', volver: 'Volver a empresas' },
  { patron: '/catalogos/categorias', nombre: 'Categorías', volver: 'Volver a categorías' },
  { patron: '/catalogos/preguntas', nombre: 'Preguntas', volver: 'Volver a preguntas' },
  { patron: '/catalogos/preguntas/nueva', nombre: 'Nueva pregunta' },
  { patron: '/catalogos/preguntas/:id', nombre: 'Pregunta', volver: 'Volver a la pregunta' },
  { patron: '/catalogos/peligros', nombre: 'Peligros y plantillas', volver: 'Volver a peligros y plantillas' },
  { patron: '/catalogos/peligros/:id', nombre: 'Peligro', volver: 'Volver al peligro' },
  { patron: '/catalogos/rubros', nombre: 'Rubros', volver: 'Volver a rubros' },
  { patron: '/catalogos/documentos', nombre: 'Plantillas de documento', volver: 'Volver a plantillas de documento' },
  { patron: '/catalogos/documentos/nuevo', nombre: 'Nueva plantilla de documento' },
  { patron: '/catalogos/documentos/:id', nombre: 'Plantilla de documento', volver: 'Volver a la plantilla' },
]

// Las rutas fijas ("/riesgos/nuevo") se prueban antes que las de parámetro ("/riesgos/:id").
const ORDENADAS = [...PANTALLAS.filter((p) => !p.patron.includes(':')), ...PANTALLAS.filter((p) => p.patron.includes(':'))]

/** La pantalla de una ruta (`pathname`, sin query), si está en la lista. */
export const pantallaDe = (pathname: string): Pantalla | undefined => ORDENADAS.find((p) => matchPath(p.patron, pathname))

/** Título de la pestaña para una ruta: "{nombre} · Norvia", o "Norvia" si no está en la lista. */
export function tituloPestana(pathname: string): string {
  const nombre = pantallaDe(pathname)?.nombre
  return !nombre || nombre === 'Norvia' ? 'Norvia' : `${nombre} · Norvia`
}

/** Rutas sin sesión: "Volver" no cruza entre esta zona y la de sesión (no se "vuelve" del inicio al login). */
const PUBLICAS = new Set(['/', '/login', '/plataforma', '/registro', '/verificar-cuenta', '/recuperar-contrasena', '/restablecer-contrasena'])
export const esPublica = (pathname: string) => PUBLICAS.has(pathname)
