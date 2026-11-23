import { api } from '@/shared/api'

export interface VerificarCuentaRequest {
  email: string
  contrasenaTemporal: string
  contrasenaNueva: string
  contrasenaNuevaRepetida: string
}

/** POST /api/auth/cuenta/verificar: activa la cuenta y fija la contraseña definitiva (204). */
export const verificarCuenta = (datos: VerificarCuentaRequest) => api.post<void>('/auth/cuenta/verificar', datos)
