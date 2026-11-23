import type { Location } from 'react-router-dom'
import type { MotivoFinSesion } from '@/shared/api'

/** Lo que RutaPrivada le pasa al login: a dónde volver y por qué se pidió iniciar sesión. */
export interface EstadoLogin {
  desde?: string
  motivo?: MotivoFinSesion
}

export const RUTA_LOGIN = '/login'
export const RUTA_INICIO = '/inicio'
/** Primer ingreso con contraseña temporal: se exige cambiarla antes de continuar. */
export const RUTA_CAMBIAR_CONTRASENA = '/cambiar-contrasena'

/** Solo rutas internas ("/algo"), para no redirigir fuera de Norvia. */
export function destinoSeguro(estado: unknown): string {
  const desde = (estado as EstadoLogin | null)?.desde
  return typeof desde === 'string' && desde.startsWith('/') && !desde.startsWith('//') ? desde : RUTA_INICIO
}

export const rutaActual = (location: Location) => `${location.pathname}${location.search}${location.hash}`
