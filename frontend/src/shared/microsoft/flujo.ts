/**
 * Inicio de sesión y registro con Microsoft (D29, HU-119, HU-120): OAuth 2.0 con código de autorización y PKCE.
 * El navegador solo lleva a la persona a Microsoft y recibe el `code`; quien lo canjea y valida la identidad es
 * el backend. Aquí nunca hay un secreto: `VITE_MICROSOFT_CLIENT_ID` y la URL de retorno son públicos.
 */

import { extraerError } from '@/shared/api'

export type PropositoMicrosoft = 'login' | 'registro'

/** Lo que hay que recordar mientras la persona está en Microsoft (sessionStorage: solo esta pestaña). */
interface Pendiente {
  proposito: PropositoMicrosoft
  verifier: string
  /** Empresa a la que se quiere entrar (solo en `login`), cifrada tal como la dio el backend. */
  idEmpresa?: string
}

const CLAVE_PENDIENTE = 'norvia.microsoft.pendiente'

export const REDIRECT_URI =
  import.meta.env.VITE_MICROSOFT_REDIRECT_URI ?? `${window.location.origin}/auth/microsoft/callback`

const clientId: string | undefined = import.meta.env.VITE_MICROSOFT_CLIENT_ID
const tenant: string = import.meta.env.VITE_MICROSOFT_TENANT ?? 'common'

/** false si falta `VITE_MICROSOFT_CLIENT_ID`: el botón avisa en lugar de llevar a una página de error de Microsoft. */
export const microsoftConfigurado = Boolean(clientId)

const base64Url = (bytes: Uint8Array) =>
  btoa(String.fromCharCode(...bytes)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')

const aleatorio = (bytes: number) => base64Url(crypto.getRandomValues(new Uint8Array(bytes)))

/** Lleva a la persona a Microsoft. Al volver, `/auth/microsoft/callback` sigue el flujo. */
export async function iniciarConMicrosoft(proposito: PropositoMicrosoft, idEmpresa?: string) {
  if (!clientId) throw new Error('Falta VITE_MICROSOFT_CLIENT_ID.')

  const verifier = aleatorio(64)
  const state = aleatorio(16)
  const desafio = base64Url(new Uint8Array(await crypto.subtle.digest('SHA-256', new TextEncoder().encode(verifier))))

  const pendiente: Pendiente = { proposito, verifier, idEmpresa }
  sessionStorage.setItem(`${CLAVE_PENDIENTE}.${state}`, JSON.stringify(pendiente))

  const parametros = new URLSearchParams({
    client_id: clientId,
    response_type: 'code',
    redirect_uri: REDIRECT_URI,
    response_mode: 'query',
    scope: 'openid profile email',
    state,
    code_challenge: desafio,
    code_challenge_method: 'S256',
    // Siempre se elige la cuenta: evita entrar sin querer con otra cuenta de Microsoft ya abierta.
    prompt: 'select_account',
  })
  window.location.assign(`https://login.microsoftonline.com/${tenant}/oauth2/v2.0/authorize?${parametros}`)
}

/** Recupera (y borra) lo guardado para ese `state`. null si no coincide: el retorno no lo inició esta pestaña. */
export function tomarPendiente(state: string | null): Pendiente | null {
  if (!state) return null
  const clave = `${CLAVE_PENDIENTE}.${state}`
  try {
    const valor = sessionStorage.getItem(clave)
    sessionStorage.removeItem(clave)
    return valor ? (JSON.parse(valor) as Pendiente) : null
  } catch {
    return null
  }
}

/** El backend responde `auth.identidad_invalida` cuando el token de identidad de Microsoft venció (30 minutos). */
export const esIdentidadVencida = (causa: unknown) => extraerError(causa).codigo === 'auth.identidad_invalida'
