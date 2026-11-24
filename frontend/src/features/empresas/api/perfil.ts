import { api } from '@/shared/api'
import type { ActualizarPerfilRequest, PerfilEmpresa } from '../types/perfil'

/** Perfil de la empresa de la sesión (HU-026). */
export const obtenerPerfil = (signal: AbortSignal) => api.get<PerfilEmpresa>('/empresas/perfil', { signal }).then((r) => r.data)

export const actualizarPerfil = (datos: ActualizarPerfilRequest) =>
  api.patch<PerfilEmpresa>('/empresas/perfil', datos).then((r) => r.data)
