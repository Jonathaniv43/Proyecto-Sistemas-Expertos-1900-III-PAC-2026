import { api } from '@/shared/api'
import type { CrearVinculoRequest, CrearVinculoResponse } from '../types/vinculos'

export const crearVinculo = (datos: CrearVinculoRequest) =>
  api.post<CrearVinculoResponse>('/vinculos', datos).then((r) => r.data)
