import { api, renovarSesion, sesion, type MotivoFinSesion, type UsuarioSesion } from '@/shared/api'

/**
 * Estado de la sesión para la interfaz.
 * - `verificando`: al abrir la app con un refresh guardado, mientras se renueva el access token.
 * - `sin_conexion`: hay refresh pero no se pudo hablar con el backend; se puede reintentar.
 * - `activa` / `sin_sesion`.
 */
export type EstadoSesion =
  | { estado: 'verificando' }
  | { estado: 'sin_conexion' }
  | { estado: 'sin_sesion'; motivo?: MotivoFinSesion }
  | { estado: 'activa'; usuario: UsuarioSesion }

let actual: EstadoSesion = sesion.hayRefresh() ? { estado: 'verificando' } : { estado: 'sin_sesion' }
let ultimoMotivo: MotivoFinSesion | undefined
const oyentes = new Set<() => void>()

function fijar(nuevo: EstadoSesion) {
  actual = nuevo
  oyentes.forEach((oyente) => oyente())
}

sesion.alTerminar((motivo) => {
  ultimoMotivo = motivo
})

sesion.alCambiar(() => {
  const usuario = sesion.obtenerUsuario()
  if (usuario) fijar({ estado: 'activa', usuario })
  else fijar({ estado: 'sin_sesion', motivo: ultimoMotivo })
})

export const estadoSesion = {
  obtener: () => actual,
  suscribir(oyente: () => void) {
    oyentes.add(oyente)
    return () => {
      oyentes.delete(oyente)
    }
  },
}

/**
 * Al abrir o recargar la app: si hay refresh guardado, pide un access token nuevo.
 * Se llama una vez desde app/ (y de nuevo con "Reintentar" si no hubo conexión).
 */
export async function restaurarSesion() {
  if (sesion.obtenerAccessToken() || !sesion.hayRefresh()) return
  fijar({ estado: 'verificando' })
  const resultado = await renovarSesion()
  if (resultado === 'rechazada') sesion.terminar('expirada')
  else if (resultado === 'sin_conexion') fijar({ estado: 'sin_conexion' })
}

/** Cierra la sesión en el backend (borra el refresh, RN-66) y en el navegador. */
export async function cerrarSesion() {
  const refreshToken = sesion.obtenerRefreshToken()
  try {
    if (refreshToken) await api.post('/auth/logout', { refreshToken })
  } catch {
    // Aunque el backend no responda, la sesión local se cierra igual.
  } finally {
    sesion.terminar('cerrada')
  }
}
