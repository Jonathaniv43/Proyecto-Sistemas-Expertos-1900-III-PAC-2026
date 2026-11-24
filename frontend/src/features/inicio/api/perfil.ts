import { api } from '@/shared/api'

/** Solo lo que el inicio necesita del perfil (GET /api/empresas/perfil, HU-026). */
export const obtenerPerfilCompleto = (signal: AbortSignal) =>
  api.get<{ perfilCompleto: boolean }>('/empresas/perfil', { signal }).then((r) => r.data.perfilCompleto)
