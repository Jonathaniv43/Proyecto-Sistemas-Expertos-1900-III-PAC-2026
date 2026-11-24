import { api, type SesionResponse } from '@/shared/api'
import type { EmpresaLogin } from '@/shared/sesion'

/** Respuesta de por-rtn: `idEmpresa` cifrado (va tal cual al login), razón social y RTN. */
export type EmpresaPorRtn = EmpresaLogin

/** GET /api/empresas/por-rtn (anónimo): 404 si no hay una empresa activa con ese RTN. */
export const resolverEmpresaPorRtn = (rtn: string) =>
  api.get<EmpresaPorRtn>('/empresas/por-rtn', { params: { rtn } }).then((r) => r.data)

export const iniciarSesionEmpresa = (datos: { email: string; password: string; idEmpresa: string }) =>
  api.post<SesionResponse>('/auth/login/empresa', datos).then((r) => r.data)

export const iniciarSesionPlataforma = (datos: { email: string; password: string }) =>
  api.post<SesionResponse>('/auth/login/plataforma', datos).then((r) => r.data)

/** POST /api/auth/contrasena/reestablecer: cambia la contraseña y cierra todas las sesiones (204). */
export const cambiarContrasena = (datos: { contrasenaActual: string; contrasenaNueva: string }) =>
  api.post<void>('/auth/contrasena/reestablecer', datos)
