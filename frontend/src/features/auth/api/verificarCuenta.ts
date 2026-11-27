import { api } from '@/shared/api'

export interface VerificarCuentaRequest {
  email: string
  codigo: string
  contrasenaNueva: string
  contrasenaNuevaRepetida: string
}

/** POST /api/auth/cuenta/verificar: confirma el código de 6 dígitos y fija la contraseña definitiva (204). */
export const verificarCuenta = (datos: VerificarCuentaRequest) => api.post<void>('/auth/cuenta/verificar', datos)

/** POST /api/auth/cuenta/verificacion/reenvio: pide un código nuevo. Respuesta siempre genérica (202). */
export const reenviarVerificacionCuenta = (email: string) =>
  api.post<{ mensaje: string }>('/auth/cuenta/verificacion/reenvio', { email }).then((r) => r.data)
