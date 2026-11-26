import axios from 'axios'

/**
 * Extractor universal de errores. Convierte cualquier fallo de una petición en un `ErrorApi`:
 * un mensaje general y, si el backend devolvió ValidationProblemDetails, los mensajes por campo.
 *
 * Formatos que produce el backend (RFC 7807, application/problem+json):
 * - Validación (FluentValidation): `{ title, status: 400, errors: { "Email": ["…"] } }`
 * - Fallo de negocio (Resultado): `{ title, status, codigo }`
 * - Error no controlado: `{ title, status: 500 }`
 */
export interface ErrorApi {
  /** Mensaje para mostrar a la persona. */
  mensaje: string
  /** Mensajes por campo, con la ruta en camelCase como la usa el formulario ("personas.0.correo"). */
  campos: Record<string, string[]>
  /** Estado HTTP; undefined si no hubo respuesta (sin conexión, tiempo agotado). */
  status?: number
  /** Código de error de negocio del backend (extensión `codigo`), si lo trae. */
  codigo?: string
  /** La petición se canceló a propósito (p. ej. la pantalla se cerró); no se muestra. */
  cancelada: boolean
}

interface ProblemDetails {
  title?: string
  status?: number
  codigo?: string
  errors?: Record<string, string[]>
}

const MENSAJES = {
  sinConexion: 'No se pudo conectar con Norvia. Revisa tu conexión e inténtalo de nuevo.',
  tiempoAgotado: 'La operación tardó demasiado. Inténtalo de nuevo.',
  noAutorizado: 'Tu sesión terminó. Vuelve a iniciar sesión para continuar.',
  prohibido: 'No tienes permiso para realizar esta acción.',
  noEncontrado: 'No encontramos lo que buscas.',
  servidor: 'Ocurrió un error inesperado. Inténtalo de nuevo en unos minutos.',
  generico: 'No se pudo completar la operación. Inténtalo de nuevo.',
} as const

/** "Personas[0].Correo" → "personas.0.correo" (formato de ruta de React Hook Form). */
export function rutaDeCampo(propiedad: string): string {
  return propiedad
    .replace(/\[(\d+)\]/g, '.$1')
    .split('.')
    .filter(Boolean)
    .map((parte) => parte.charAt(0).toLowerCase() + parte.slice(1))
    .join('.')
}

function esProblemDetails(dato: unknown): dato is ProblemDetails {
  return typeof dato === 'object' && dato !== null && ('title' in dato || 'errors' in dato)
}

function mensajePorEstado(status: number): string {
  if (status === 401) return MENSAJES.noAutorizado
  if (status === 403) return MENSAJES.prohibido
  if (status === 404) return MENSAJES.noEncontrado
  if (status >= 500) return MENSAJES.servidor
  return MENSAJES.generico
}

export function extraerError(error: unknown): ErrorApi {
  const base: ErrorApi = { mensaje: MENSAJES.generico, campos: {}, cancelada: false }

  if (axios.isCancel(error)) return { ...base, mensaje: '', cancelada: true }
  if (!axios.isAxiosError(error)) return base

  if (!error.response) {
    const tiempoAgotado = error.code === 'ECONNABORTED' || error.code === 'ETIMEDOUT'
    return { ...base, mensaje: tiempoAgotado ? MENSAJES.tiempoAgotado : MENSAJES.sinConexion }
  }

  const { status, data } = error.response
  const resultado: ErrorApi = { ...base, status, mensaje: mensajePorEstado(status) }
  if (!esProblemDetails(data)) return resultado

  if (data.errors) {
    for (const [propiedad, mensajes] of Object.entries(data.errors)) {
      resultado.campos[rutaDeCampo(propiedad)] = mensajes
    }
  }

  // El título del backend ya está redactado para la persona, salvo en 500 (genérico) y 403/404:
  // para no confirmar si un recurso existe en otra empresa, esos usan el mensaje fijo.
  if (data.title && status < 500 && status !== 403 && status !== 404) {
    resultado.mensaje = data.title
  }
  resultado.codigo = data.codigo
  return resultado
}
