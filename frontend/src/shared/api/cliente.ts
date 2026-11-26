import axios, { AxiosHeaders, type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { sesion, type SesionResponse } from './sesion'

/**
 * Cliente HTTP único de la app. Las features llaman a la API solo a través de `api`.
 *
 * - Adjunta el access token como `Authorization: Bearer`.
 * - Ante un 401 renueva la sesión con el refresh token (una sola renovación aunque fallen
 *   varias peticiones a la vez) y repite la petición una vez.
 * - Si el backend rechaza el refresh, termina la sesión local con motivo `expirada`; si no hay
 *   conexión, la conserva y la petición falla con su error.
 *
 * Los errores se convierten a mensajes con `extraerError` (./errores).
 */

/** En desarrollo, `/api` pasa por el proxy de Vite; en producción se define VITE_API_URL. */
const baseURL = import.meta.env.VITE_API_URL ?? '/api'
const TIEMPO_LIMITE_MS = 20_000

export const api = axios.create({ baseURL, timeout: TIEMPO_LIMITE_MS })

/** Cliente sin interceptores, solo para renovar la sesión (evita ciclos de 401 → refresh → 401). */
const clienteRefresh = axios.create({ baseURL, timeout: TIEMPO_LIMITE_MS })

/** Rutas donde un 401 significa "credenciales inválidas", no "sesión vencida". */
const RUTAS_SIN_RENOVACION = ['/auth/login/', '/auth/refresh', '/auth/logout']

type ConfigConReintento = InternalAxiosRequestConfig & { _reintentada?: boolean }

api.interceptors.request.use((config) => {
  const token = sesion.obtenerAccessToken()
  if (token) {
    config.headers = AxiosHeaders.from(config.headers)
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

/**
 * - `renovada`: hay access token nuevo.
 * - `rechazada`: el backend no aceptó el refresh (vencido, revocado o no hay): la sesión terminó.
 * - `sin_conexion`: no se pudo hablar con el backend (red, caída, 5xx): la sesión NO se cierra.
 * - `reemplazada`: el refresh guardado es de otra sesión (otra pestaña entró a otra empresa); esta
 *   sesión ya terminó con ese motivo.
 */
export type ResultadoRenovacion = 'renovada' | 'rechazada' | 'sin_conexion' | 'reemplazada'

let renovacionEnCurso: Promise<ResultadoRenovacion> | null = null

async function pedirRefresh(refreshToken: string): Promise<ResultadoRenovacion> {
  try {
    const { data } = await clienteRefresh.post<SesionResponse>('/auth/refresh', { refreshToken })
    return sesion.renovar(data) ? 'renovada' : 'reemplazada'
  } catch (error) {
    const status = axios.isAxiosError(error) ? error.response?.status : undefined
    return status !== undefined && status < 500 ? 'rechazada' : 'sin_conexion'
  }
}

async function renovar(): Promise<ResultadoRenovacion> {
  const usado = sesion.obtenerRefreshToken()
  if (!usado) return 'rechazada'

  const resultado = await pedirRefresh(usado)
  if (resultado !== 'rechazada') return resultado

  // Otra pestaña pudo rotar el refresh mientras tanto: si cambió, se intenta con el nuevo.
  const actual = sesion.obtenerRefreshToken()
  return actual !== null && actual !== usado ? pedirRefresh(actual) : 'rechazada'
}

/** Renueva la sesión una sola vez aunque la pidan varias peticiones a la vez. */
export function renovarSesion(): Promise<ResultadoRenovacion> {
  renovacionEnCurso ??= renovar().finally(() => {
    renovacionEnCurso = null
  })
  return renovacionEnCurso
}

api.interceptors.response.use(undefined, async (error: AxiosError) => {
  const config = error.config as ConfigConReintento | undefined
  const esRutaDeAuth = RUTAS_SIN_RENOVACION.some((ruta) => config?.url?.startsWith(ruta))

  if (error.response?.status !== 401 || !config || config._reintentada || esRutaDeAuth) {
    throw error
  }

  // Sin refresh guardado no hay nada que renovar (p. ej. nunca inició sesión).
  if (!sesion.hayRefresh()) throw error

  const resultado = await renovarSesion()
  if (resultado === 'renovada') {
    config._reintentada = true
    return api(config)
  }

  // Sin conexión se conserva la sesión: la petición falla y la pantalla ofrece reintentar.
  if (resultado === 'rechazada') sesion.terminar('expirada')
  throw error
})
