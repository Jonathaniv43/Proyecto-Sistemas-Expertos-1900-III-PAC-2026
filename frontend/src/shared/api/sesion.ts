/**
 * Dónde vive la sesión en el navegador.
 *
 * - Access token: solo en memoria (dura minutos; se pierde al recargar y se renueva con el refresh).
 * - Refresh token: en localStorage, para sobrevivir recargas y pestañas nuevas. Rota en cada uso
 *   y se borra al cerrar sesión (RN-66).
 *
 * Los datos de la persona (tipo de sesión, rol, vínculo) se leen del access token. Solo sirven para
 * decidir qué mostrar: quien autoriza de verdad es el backend.
 *
 * Una empresa por navegador (HU-019): todas las pestañas comparten el refresh. Si en otra pestaña se
 * inicia sesión en otra empresa (o con otra persona), esta pestaña cierra la suya con motivo
 * `reemplazada`, para que nunca se mezclen datos de dos empresas.
 */

/** Respuesta de login y refresh (SesionResponseDto del backend). */
export interface SesionResponse {
  accessToken: string
  refreshToken: string
  /** Segundos de vida del access token. */
  expiresIn: number
}

/**
 * - `cerrada`: la persona cerró sesión (aquí o en otra pestaña).
 * - `expirada`: el backend ya no acepta la sesión.
 * - `reemplazada`: se inició sesión en otra empresa en este navegador.
 * - `contrasena_cambiada`: cambió su contraseña; el backend cerró todas sus sesiones (HU-114).
 */
export type MotivoFinSesion = 'cerrada' | 'expirada' | 'reemplazada' | 'contrasena_cambiada'

/** Empresa de la sesión, para mostrarla (viene de la consulta por RTN al iniciar sesión). */
export interface EmpresaSesion {
  razonSocial: string
  rtn: string
}

export type TipoSesion = 'empresa' | 'plataforma'

/** Lo que el front sabe de quien inició sesión (claims del JWT). */
export interface UsuarioSesion {
  tipo: TipoSesion
  /** Código del rol: admin_empresa, responsable_sst, colaborador, encuestado (empresa) o rol de staff. */
  rol: string | null
  /** VinculoEmpresa de la sesión; solo en sesiones de empresa. */
  vinculoId: string | null
  /** Correo de la cuenta (claim `email`): a dónde se envía el enlace para cambiar la contraseña (HU-022). */
  email: string | null
  /** Solo en sesiones de empresa iniciadas desde este navegador. */
  empresa: EmpresaSesion | null
}

const CLAVE_REFRESH = 'norvia.refreshToken'
/** Identifica cada inicio de sesión: cambia al iniciar sesión, no al renovar. */
const CLAVE_SESION_ID = 'norvia.sesionId'
const CLAVE_EMPRESA = 'norvia.empresa'
/** Por qué terminó la última sesión: las otras pestañas lo leen para explicar el cierre. */
const CLAVE_MOTIVO_FIN = 'norvia.motivoFin'
const MOTIVOS_FIN: readonly MotivoFinSesion[] = ['cerrada', 'expirada', 'reemplazada', 'contrasena_cambiada']
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

function escribir(clave: string, valor: string | null) {
  try {
    if (valor) localStorage.setItem(clave, valor)
    else localStorage.removeItem(clave)
  } catch {
    // sin almacenamiento persistente
  }
}

const escribirRefresh = (valor: string | null) => escribir(CLAVE_REFRESH, valor)

function leer(clave: string): string | null {
  try {
    return localStorage.getItem(clave)
  } catch {
    return null
  }
}

function leerEmpresa(): EmpresaSesion | null {
  try {
    const valor = JSON.parse(leer(CLAVE_EMPRESA) ?? 'null') as EmpresaSesion | null
    return valor && typeof valor.razonSocial === 'string' && typeof valor.rtn === 'string' ? valor : null
  } catch {
    return null
  }
}

/** Id de este inicio de sesión en esta pestaña, para notar si otra pestaña inicia otro. */
let sesionIdPropio: string | null = leer(CLAVE_SESION_ID)

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

function usuarioDe(token: string): UsuarioSesion | null {
  const claims = leerClaims(token)
  if (!claims) return null
  const texto = (valor: unknown) => (typeof valor === 'string' ? valor : null)
  return {
    tipo: claims.tipo_sesion === 'plataforma' ? 'plataforma' : 'empresa',
    rol: texto(claims.role) ?? texto(claims[CLAIM_ROL_LARGO]),
    vinculoId: texto(claims.vinculo_id),
    email: texto(claims.email),
    empresa: claims.tipo_sesion === 'plataforma' ? null : leerEmpresa(),
  }
}

let empresaTerminada: EmpresaSesion | null = null

const avisarCambio = () => oyentesCambio.forEach((oyente) => oyente())

export const sesion = {
  obtenerAccessToken: () => accessToken,
  obtenerRefreshToken: leerRefresh,
  hayRefresh: () => leerRefresh() !== null,
  /** Persona de la sesión actual, o null si no hay access token. Mismo objeto hasta que cambie. */
  obtenerUsuario: () => usuario,

  /** Inicio de sesión: guarda los tokens y marca un inicio nuevo (las demás pestañas lo notan). */
  iniciar(respuesta: SesionResponse, empresa: EmpresaSesion | null) {
    empresaTerminada = null
    escribir(CLAVE_MOTIVO_FIN, null)
    sesionIdPropio = crypto.randomUUID()
    escribir(CLAVE_SESION_ID, sesionIdPropio)
    escribir(CLAVE_EMPRESA, empresa ? JSON.stringify(empresa) : null)
    accessToken = respuesta.accessToken
    usuario = usuarioDe(respuesta.accessToken)
    escribirRefresh(respuesta.refreshToken)
    avisarCambio()
  },

  /**
   * Renovación: guarda los tokens nuevos. Si el refresh guardado ya era de otra sesión (otra pestaña
   * inició sesión en otra empresa), no los usa: termina esta sesión y devuelve false.
   */
  renovar(respuesta: SesionResponse): boolean {
    const nuevo = usuarioDe(respuesta.accessToken)
    const otraSesion = usuario !== null && (nuevo?.vinculoId !== usuario.vinculoId || nuevo?.tipo !== usuario.tipo)
    if (otraSesion) {
      // El refresh nuevo pertenece a la otra pestaña: se conserva para ella.
      escribirRefresh(respuesta.refreshToken)
      this.terminar('reemplazada', { conservarAlmacenamiento: true })
      return false
    }
    sesionIdPropio = leer(CLAVE_SESION_ID)
    accessToken = respuesta.accessToken
    usuario = nuevo
    escribirRefresh(respuesta.refreshToken)
    avisarCambio()
    return true
  },

  /** La razón social cambió en el perfil (HU-026): se actualiza la que muestra el encabezado. */
  actualizarRazonSocial(razonSocial: string) {
    if (!usuario?.empresa || usuario.empresa.razonSocial === razonSocial) return
    const empresa = { ...usuario.empresa, razonSocial }
    escribir(CLAVE_EMPRESA, JSON.stringify(empresa))
    usuario = { ...usuario, empresa }
    avisarCambio()
  },

  /**
   * Borra la sesión de esta pestaña y avisa a quien escuche (p. ej. el router, para ir al login).
   * Con `conservarAlmacenamiento` no toca lo guardado: es de la sesión de otra pestaña.
   */
  terminar(motivo: MotivoFinSesion, { conservarAlmacenamiento = false } = {}) {
    // Al recargar con la sesión vencida todavía no hay usuario: la empresa se lee de lo guardado.
    // Si termina dos veces (p. ej. la renovación fallida en modo estricto), no se pierde la que se conocía.
    empresaTerminada = usuario?.empresa ?? leerEmpresa() ?? empresaTerminada
    accessToken = null
    usuario = null
    if (!conservarAlmacenamiento) {
      // Antes de borrar el refresh: al notarlo, las otras pestañas leen el motivo (p. ej. contraseña cambiada).
      escribir(CLAVE_MOTIVO_FIN, motivo)
      escribirRefresh(null)
      escribir(CLAVE_SESION_ID, null)
      escribir(CLAVE_EMPRESA, null)
      sesionIdPropio = null
    }
    oyentesFin.forEach((oyente) => oyente(motivo))
    avisarCambio()
  },

  /** Empresa de la última sesión que terminó en esta pestaña (HU-019: no volver a una página de otra empresa). */
  empresaTerminada: () => empresaTerminada,

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

// Lo que hacen otras pestañas:
// - cierran sesión (borran el refresh): esta pestaña también la cierra, con el mismo motivo;
// - inician sesión (cambia el id de sesión): esta pestaña cierra la suya sin borrar la nueva.
if (typeof window !== 'undefined') {
  window.addEventListener('storage', (evento) => {
    if (accessToken === null) return
    const borrado = evento.key === null || (evento.key === CLAVE_REFRESH && evento.newValue === null)
    if (borrado) {
      const motivo = leer(CLAVE_MOTIVO_FIN) as MotivoFinSesion | null
      sesion.terminar(motivo && MOTIVOS_FIN.includes(motivo) ? motivo : 'cerrada', { conservarAlmacenamiento: true })
      return
    }
    if (evento.key === CLAVE_SESION_ID && evento.newValue !== null && evento.newValue !== sesionIdPropio) {
      sesion.terminar('reemplazada', { conservarAlmacenamiento: true })
    }
  })
}
