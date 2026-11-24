/**
 * Última empresa resuelta por RTN en este navegador (GET /api/empresas/por-rtn), solo para el login:
 * mostrar su nombre en `/login?empresa=…` aunque se recargue, y ofrecerla como atajo al buscar.
 * El `idEmpresa` se guarda tal como lo da el backend (cifrado); el front nunca lo descifra.
 */
export interface EmpresaLogin {
  /** Identificador de empresa cifrado que pide el login. */
  idEmpresa: string
  razonSocial: string
  rtn: string
}

const CLAVE = 'norvia.ultimaEmpresa'

export function leerUltimaEmpresa(): EmpresaLogin | null {
  try {
    const valor = JSON.parse(localStorage.getItem(CLAVE) ?? 'null') as Partial<EmpresaLogin> | null
    return typeof valor?.idEmpresa === 'string' && typeof valor.razonSocial === 'string' && typeof valor.rtn === 'string'
      ? { idEmpresa: valor.idEmpresa, razonSocial: valor.razonSocial, rtn: valor.rtn }
      : null
  } catch {
    return null
  }
}

export function recordarEmpresa({ idEmpresa, razonSocial, rtn }: EmpresaLogin) {
  try {
    localStorage.setItem(CLAVE, JSON.stringify({ idEmpresa, razonSocial, rtn }))
  } catch {
    // sin almacenamiento: solo se pierde la comodidad
  }
}
