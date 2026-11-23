import type { SesionResponseDto } from '../types/sesion.types'

type Listener = () => void

let accessToken: string | null = null
let expiresAt: number | null = null
let debeCambiarPassword = false
const listeners = new Set<Listener>()

function notificar() {
  for (const listener of listeners) listener()
}

/** Access token en memoria. El refresh vive en cookie HttpOnly. */
export const sesionStore = {
  obtenerAccessToken(): string | null {
    return accessToken
  },

  obtenerDebeCambiarPassword(): boolean {
    return debeCambiarPassword
  },

  obtenerExpiresAt(): number | null {
    return expiresAt
  },

  aplicarSesion(sesion: SesionResponseDto) {
    accessToken = sesion.accessToken
    debeCambiarPassword = sesion.debeCambiarPassword
    expiresAt = Date.now() + sesion.expiresIn * 1000
    notificar()
  },

  limpiar() {
    accessToken = null
    expiresAt = null
    debeCambiarPassword = false
    notificar()
  },

  suscribir(listener: Listener): () => void {
    listeners.add(listener)
    return () => listeners.delete(listener)
  },
}
