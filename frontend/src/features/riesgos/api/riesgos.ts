import { api } from '@/shared/api'
import type { CrearRiesgoRequest, PeligroDisponible, Riesgo } from '../types/riesgos'

/** Riesgos de la matriz (confirmados), los más recientes primero (HU-036). */
export const listarRiesgos = (signal: AbortSignal) => api.get<Riesgo[]>('/riesgos', { signal }).then((r) => r.data)

export const listarPeligrosDisponibles = (signal: AbortSignal) =>
  api.get<PeligroDisponible[]>('/riesgos/peligros-disponibles', { signal }).then((r) => r.data)

/** Agrega un riesgo a la matriz. 409 `riesgos.duplicado` si ya está con la misma actividad. */
export const crearRiesgo = (datos: CrearRiesgoRequest) => api.post<Riesgo>('/riesgos', datos).then((r) => r.data)
