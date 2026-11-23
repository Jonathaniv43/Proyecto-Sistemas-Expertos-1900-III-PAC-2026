/** Recuerda el RTN del último ingreso en este navegador, solo para rellenar el login. */
const CLAVE = 'norvia.ultimoRtn'

export function leerUltimoRtn(): string {
  try {
    return localStorage.getItem(CLAVE) ?? ''
  } catch {
    return ''
  }
}

export function recordarRtn(rtn: string) {
  try {
    localStorage.setItem(CLAVE, rtn)
  } catch {
    // sin almacenamiento: solo se pierde la comodidad
  }
}
