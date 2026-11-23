import axios from 'axios'
import type { ErrorBackendParseado, ProblemDetails } from '../types/errors.types'

/** Mensaje único para banners / toasts. */
export function extraerMensajeErrorBackend(error: unknown): string {
  return parsearErrorBackend(error).mensaje
}

/** Mapa por campo para React Hook Form (`setError`). */
export function extraerErroresPorCampo(error: unknown): Record<string, string[]> | undefined {
  return parsearErrorBackend(error).erroresPorCampo
}

/**
 * Lee el contrato fijo de Norvia:
 * - sin response → red / CORS
 * - ValidationProblemDetails.errors → mensajes por campo
 * - ProblemDetails.title / detail → mensaje general
 */
export function parsearErrorBackend(error: unknown): ErrorBackendParseado {
  if (!axios.isAxiosError(error)) {
    return {
      mensaje: error instanceof Error ? error.message : 'Error inesperado en la aplicación.',
    }
  }

  if (!error.response) {
    return {
      mensaje:
        'Sin respuesta del servidor (.NET apagado o bloqueo de seguridad CORS en el navegador).',
    }
  }

  const status = error.response.status
  const data = error.response.data as ProblemDetails | string | undefined

  if (data && typeof data === 'object' && data.errors) {
    const erroresPorCampo = data.errors
    const lista = Object.values(erroresPorCampo).flat()
    return {
      mensaje: `[HTTP ${status}] ${lista.join(' | ')}`,
      erroresPorCampo,
      status,
    }
  }

  if (typeof data === 'string' && data.length > 0) {
    return { mensaje: `[HTTP ${status}] ${data}`, status }
  }

  if (data && typeof data === 'object') {
    const texto = data.detail ?? data.title
    if (texto) {
      return { mensaje: `[HTTP ${status}] ${texto}`, status }
    }
  }

  return {
    mensaje: `[HTTP ${status}] El servidor .NET rechazó la operación.`,
    status,
  }
}
