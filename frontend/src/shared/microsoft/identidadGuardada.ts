/** Estados que devuelve POST /api/auth/microsoft/identidad. */
export type EstadoIdentidad = 'sin_cuenta' | 'cuenta_vinculada' | 'cuenta_con_contrasena'

/** Respuesta de POST /api/auth/microsoft/identidad (IdentidadMicrosoftResponseDto). */
export interface IdentidadMicrosoft {
  /** Token firmado de corta vida: lo único que acompaña los pasos siguientes. */
  tokenIdentidad: string
  /** Correo verificado por Microsoft: va bloqueado en el registro (RN-84). */
  email: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
  estado: EstadoIdentidad
  /** true si los nombres vienen de la persona ya guardada en Norvia: no se editan (RN-85). */
  personaExistente: boolean
  /** true cuando el dueño del correo lo confirmó con el código de 6 dígitos (RN-86). Solo se guarda en este navegador. */
  correoVerificado?: boolean
}

const CLAVE = 'norvia.microsoft.identidad'

/** La identidad validada pasa de la página de retorno al formulario de registro (sessionStorage: esta pestaña). */
export function guardarIdentidad(identidad: IdentidadMicrosoft) {
  try {
    sessionStorage.setItem(CLAVE, JSON.stringify(identidad))
  } catch {
    // sin almacenamiento: el formulario pedirá empezar de nuevo
  }
}

export function leerIdentidad(): IdentidadMicrosoft | null {
  try {
    const valor = JSON.parse(sessionStorage.getItem(CLAVE) ?? 'null') as Partial<IdentidadMicrosoft> | null
    return typeof valor?.tokenIdentidad === 'string' && typeof valor.email === 'string'
      ? (valor as IdentidadMicrosoft)
      : null
  } catch {
    return null
  }
}

export function olvidarIdentidad() {
  try {
    sessionStorage.removeItem(CLAVE)
  } catch {
    // nada que borrar
  }
}
