/**
 * Dónde vive la sesión en el navegador.
 *
 * - Access token: solo en memoria (dura minutos; se pierde al recargar y se renueva con el refresh).
 * - Refresh token: en localStorage, para sobrevivir recargas y pestañas nuevas. Rota en cada uso
 *   y se borra al cerrar sesión (RN-66).
 *
 * Los datos de la persona (tipo de sesión, rol, vínculo) se leen del access token. Solo sirven para
 * decidir qué mostrar: quien autoriza de verdad es el backend.
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

export type TipoSesion = 'empresa' | 'plataforma'

/** Lo que el front sabe de quien inició sesión (claims del JWT). */
export interface UsuarioSesion {
  tipo: TipoSesion
  /** Código del rol: admin_empresa, responsable_sst, colaborador, encuestado (empresa) o rol de staff. */
  rol: string | null
  /** VinculoEmpresa de la sesión; solo en sesiones de empresa. */
  vinculoId: string | null
  debeCambiarPassword: boolean
}

const CLAVE_REFRESH = 'norvia.refreshToken'
const CLAIM_ROL_LARGO = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

let accessToken: string | null = null
let usuario: UsuarioSesion | null = null
const oyentesFin = new Set<(motivo: MotivoFinSesion) => void>()
const oyentesCambio = new Set<() => void>()

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

/** Lee el payload del JWT (sin verificar la firma: eso lo hace el backend). */
function leerClaims(token: string): Record<string, unknown> | null {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const json = new TextDecoder().decode(Uint8Array.from(atob(payload), (c) => c.charCodeAt(0)))
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

function usuarioDe(token: string, debeCambiarPassword: boolean): UsuarioSesion | null {
  const claims = leerClaims(token)
  if (!claims) return null
  const texto = (valor: unknown) => (typeof valor === 'string' ? valor : null)
  return {
    tipo: claims.tipo_sesion === 'plataforma' ? 'plataforma' : 'empresa',
    rol: texto(claims.role) ?? texto(claims[CLAIM_ROL_LARGO]),
    vinculoId: texto(claims.vinculo_id),
    debeCambiarPassword,
  }
}

const avisarCambio = () => oyentesCambio.forEach((oyente) => oyente())

export const sesion = {
  obtenerAccessToken: () => accessToken,
  obtenerRefreshToken: leerRefresh,
  hayRefresh: () => leerRefresh() !== null,
  /** Persona de la sesión actual, o null si no hay access token. Mismo objeto hasta que cambie. */
  obtenerUsuario: () => usuario,

  /** Guarda los tokens recibidos en el login o en el refresh. */
  guardar(respuesta: SesionResponse) {
    accessToken = respuesta.accessToken
    usuario = usuarioDe(respuesta.accessToken, respuesta.debeCambiarPassword)
    escribirRefresh(respuesta.refreshToken)
    avisarCambio()
  },

  /** Marca que ya se cambió la contraseña temporal (HU-114). */
  marcarContrasenaCambiada() {
    if (usuario?.debeCambiarPassword) {
      usuario = { ...usuario, debeCambiarPassword: false }
      avisarCambio()
    }
  },

  /** Borra la sesión local y avisa a quien escuche (p. ej. el router, para ir al login). */
  terminar(motivo: MotivoFinSesion) {
    accessToken = null
    usuario = null
    escribirRefresh(null)
    oyentesFin.forEach((oyente) => oyente(motivo))
    avisarCambio()
  },

  /** Suscribe a los fines de sesión; devuelve la función para desuscribirse. */
  alTerminar(oyente: (motivo: MotivoFinSesion) => void) {
    oyentesFin.add(oyente)
    return () => {
      oyentesFin.delete(oyente)
    }
  },

  /** Suscribe a cualquier cambio (inicio, renovación, fin); devuelve la función para desuscribirse. */
  alCambiar(oyente: () => void) {
    oyentesCambio.add(oyente)
    return () => {
      oyentesCambio.delete(oyente)
    }
  },
}

// Otra pestaña cerró sesión (borró el refresh): esta pestaña también la cierra.
if (typeof window !== 'undefined') {
  window.addEventListener('storage', (evento) => {
    const borrado = evento.key === null || (evento.key === CLAVE_REFRESH && evento.newValue === null)
    if (borrado && accessToken !== null) {
      sesion.terminar('cerrada')
    }
  })
}
