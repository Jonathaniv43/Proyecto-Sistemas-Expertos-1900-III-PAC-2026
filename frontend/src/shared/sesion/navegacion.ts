import type { Location } from 'react-router-dom'
import type { MotivoFinSesion } from '@/shared/api'

/** Lo que RutaPrivada le pasa al login: a dónde volver y por qué se pidió iniciar sesión. */
export interface EstadoLogin {
  desde?: string
  motivo?: MotivoFinSesion
  /** RTN de la empresa de la sesión que terminó: solo se vuelve a `desde` si se entra a la misma (HU-019). */
  rtn?: string
}

export const RUTA_LOGIN = '/login'
export const RUTA_INICIO = '/inicio'
/** Primer ingreso con contraseña temporal: se exige cambiarla antes de continuar. */
export const RUTA_CAMBIAR_CONTRASENA = '/cambiar-contrasena'

/**
 * A dónde ir al iniciar sesión: la página guardada (`desde`) o el inicio.
 * - Solo rutas internas ("/algo"), para no redirigir fuera de Norvia.
 * - HU-019: si la sesión que terminó era de otra empresa, la página guardada es ajena: se va al inicio.
 */
export function destinoSeguro(estado: unknown, rtnNuevo?: string | null): string {
  const { desde, rtn } = (estado as EstadoLogin | null) ?? {}
  if (rtn && rtn !== rtnNuevo) return RUTA_INICIO
  return typeof desde === 'string' && desde.startsWith('/') && !desde.startsWith('//') ? desde : RUTA_INICIO
}

export const rutaActual = (location: Location) => `${location.pathname}${location.search}${location.hash}`
