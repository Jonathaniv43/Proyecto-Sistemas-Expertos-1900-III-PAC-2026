/**
 * Dónde vive la sesión en el navegador.
 *
 * - Access token: solo en memoria (dura minutos; se pierde al recargar y se renueva con el refresh).
 * - Refresh token: en localStorage, para sobrevivir recargas y pestañas nuevas. Rota en cada uso
 *   y se borra al cerrar sesión (RN-66).
 */

/** Respuesta de login y refresh (SesionResponseDto del backend). */
export interface SesionResponse {
  accessToken: string
  refreshToken: string
  /** Segundos de vida del access token. */
  expiresIn: number
  /** La cuenta tiene contraseña temporal: hay que pedir el cambio antes de continuar (HU-114). */
  debeCambiarPassword: boolean
}

export type MotivoFinSesion = 'cerrada' | 'expirada'

const CLAVE_REFRESH = 'norvia.refreshToken'

let accessToken: string | null = null
const oyentes = new Set<(motivo: MotivoFinSesion) => void>()

// localStorage puede no estar disponible (modo privado, almacenamiento bloqueado): la app sigue
// funcionando, solo que la sesión no sobrevive a una recarga.
function leerRefresh(): string | null {
  try {
    return localStorage.getItem(CLAVE_REFRESH)
  } catch {
    return null
  }
}

function escribirRefresh(valor: string | null) {
  try {
    if (valor) localStorage.setItem(CLAVE_REFRESH, valor)
    else localStorage.removeItem(CLAVE_REFRESH)
  } catch {
    // sin almacenamiento persistente
  }
}

export const sesion = {
  obtenerAccessToken: () => accessToken,
  obtenerRefreshToken: leerRefresh,
  hayRefresh: () => leerRefresh() !== null,

  /** Guarda los tokens recibidos en el login o en el refresh. */
  guardar(respuesta: SesionResponse) {
    accessToken = respuesta.accessToken
    escribirRefresh(respuesta.refreshToken)
  },

  /** Borra la sesión local y avisa a quien escuche (p. ej. el router, para ir al login). */
  terminar(motivo: MotivoFinSesion) {
    accessToken = null
    escribirRefresh(null)
    oyentes.forEach((oyente) => oyente(motivo))
  },

  /** Suscribe a los fines de sesión; devuelve la función para desuscribirse. */
  alTerminar(oyente: (motivo: MotivoFinSesion) => void) {
    oyentes.add(oyente)
    return () => {
      oyentes.delete(oyente)
    }
  },
}
