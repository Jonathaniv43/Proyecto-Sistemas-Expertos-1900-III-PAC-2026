import { api } from '@/shared/api'
import type { EmpresaPlataformaResumen } from '../types/plataforma'

/** Empresas registradas en la plataforma y cantidad de vínculos vigentes (HU-024). */
export const listarEmpresasPlataforma = (signal: AbortSignal, buscar: string) =>
  api
    .get<EmpresaPlataformaResumen[]>('/plataforma/empresas', {
      signal,
      params: buscar ? { buscar } : {},
    })
    .then((r) => r.data)
