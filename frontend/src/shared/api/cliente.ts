import axios, { AxiosHeaders, type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { sesion, type SesionResponse } from './sesion'

/**
 * Cliente HTTP único de la app. Las features llaman a la API solo a través de `api`.
 *
 * - Adjunta el access token como `Authorization: Bearer`.
 * - Ante un 401 renueva la sesión con el refresh token (una sola renovación aunque fallen
 *   varias peticiones a la vez) y repite la petición una vez.
 * - Si no se puede renovar, termina la sesión local con motivo `expirada`.
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

let renovacionEnCurso: Promise<boolean> | null = null

async function pedirRefresh(refreshToken: string): Promise<boolean> {
  try {
    const { data } = await clienteRefresh.post<SesionResponse>('/auth/refresh', { refreshToken })
    sesion.guardar(data)
    return true
  } catch {
    return false
  }
}

async function renovar(): Promise<boolean> {
  const usado = sesion.obtenerRefreshToken()
  if (!usado) return false

  if (await pedirRefresh(usado)) return true

  // Otra pestaña pudo rotar el refresh mientras tanto: si cambió, se intenta con el nuevo.
  const actual = sesion.obtenerRefreshToken()
  return actual !== null && actual !== usado && (await pedirRefresh(actual))
}

/** Renueva la sesión una sola vez aunque la pidan varias peticiones a la vez. */
export function renovarSesion(): Promise<boolean> {
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

  if (await renovarSesion()) {
    config._reintentada = true
    return api(config)
  }

  sesion.terminar('expirada')
  throw error
})
