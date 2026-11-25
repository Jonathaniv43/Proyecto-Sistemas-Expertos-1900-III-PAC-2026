import { api, type SesionResponse } from '@/shared/api'
import type { IdentidadMicrosoft } from '@/shared/microsoft/identidadGuardada'

/** POST /api/auth/microsoft/identidad (anónimo): canjea el código y dice en qué estado está la cuenta. */
export const obtenerIdentidadMicrosoft = (datos: { codigo: string; codeVerifier: string; redirectUri: string }) =>
  api.post<IdentidadMicrosoft>('/auth/microsoft/identidad', datos).then((r) => r.data)

/** POST /api/auth/login/empresa/microsoft (anónimo): mismas restricciones que el login con contraseña. */
export const iniciarSesionEmpresaMicrosoft = (datos: { tokenIdentidad: string; idEmpresa: string }) =>
  api.post<SesionResponse>('/auth/login/empresa/microsoft', datos).then((r) => r.data)

/** POST /api/auth/microsoft/vincular (204): vincula Microsoft confirmando la contraseña actual (RN-83). */
export const vincularMicrosoft = (datos: { tokenIdentidad: string; password: string }) =>
  api.post<void>('/auth/microsoft/vincular', datos)
