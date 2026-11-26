import { api } from '@/shared/api'

/** HU-022: pide el enlace para cambiar la contraseña. Responde siempre igual (202), exista o no la cuenta. */
export const solicitarCambioContrasena = (email: string) =>
  api.post<{ mensaje: string }>('/auth/contrasena/solicitud', { email }).then((r) => r.data)

/**
 * HU-022: cambia la contraseña con el token del enlace (204). La contraseña vale en todas las empresas
 * de la cuenta y se cierran todas sus sesiones. 410 (`auth.enlace_invalido`) si venció o ya se usó.
 */
export const confirmarCambioContrasena = (datos: { token: string; contrasenaNueva: string }) =>
  api.post<void>('/auth/contrasena/confirmacion', datos)
