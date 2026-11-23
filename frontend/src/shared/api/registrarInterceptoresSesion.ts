import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { apiClient } from './apiClient'
import type { InterceptoresSesion } from './types/interceptoresSesion.types'

type ConfigConRetry = InternalAxiosRequestConfig & { _retry?: boolean }

function esRutaAuth(url: string): boolean {
  return (
    url.includes('/auth/login') ||
    url.includes('/auth/refresh') ||
    url.includes('/auth/logout')
  )
}

/**
 * Mecanismo Bearer + reintento ante 401.
 * Quien inicia la app pasa obtenerToken y renovar (features/authentication).
 */
export function registrarInterceptoresSesion({ obtenerToken, renovar }: InterceptoresSesion) {
  apiClient.interceptors.request.use((config) => {
    const token = obtenerToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  apiClient.interceptors.response.use(
    (respuesta) => respuesta,
    async (error: AxiosError) => {
      const config = error.config as ConfigConRetry | undefined
      if (!config || error.response?.status !== 401 || config._retry) {
        return Promise.reject(error)
      }

      if (esRutaAuth(config.url ?? '')) {
        return Promise.reject(error)
      }

      config._retry = true
      const nuevoToken = await renovar()
      if (!nuevoToken) {
        return Promise.reject(error)
      }

      config.headers.Authorization = `Bearer ${nuevoToken}`
      return apiClient.request(config)
    },
  )
}
