import { api } from '@/shared/api'
import type { CrearRiesgoRequest, Evaluacion, PeligroDisponible, Riesgo } from '../types/riesgos'

/** Riesgos de la matriz (confirmados), los más recientes primero (HU-036). */
export const listarRiesgos = (signal: AbortSignal) => api.get<Riesgo[]>('/riesgos', { signal }).then((r) => r.data)

export const listarPeligrosDisponibles = (signal: AbortSignal) =>
  api.get<PeligroDisponible[]>('/riesgos/peligros-disponibles', { signal }).then((r) => r.data)

/** Agrega un riesgo a la matriz. 409 `riesgos.duplicado` si ya está con la misma actividad. */
export const crearRiesgo = (datos: CrearRiesgoRequest) => api.post<Riesgo>('/riesgos', datos).then((r) => r.data)

/** Un riesgo de la empresa, con su última evaluación. 404 si no existe o es de otra empresa. */
export const obtenerRiesgo = (signal: AbortSignal, id: string) => api.get<Riesgo>(`/riesgos/${id}`, { signal }).then((r) => r.data)

/** Evaluación inicial (HU-037). Se suma al historial: la anterior no se borra. 409 si el riesgo no está en la matriz. */
export const evaluarRiesgo = (id: string, datos: { probabilidad: number; severidad: number }) =>
  api.post<Evaluacion>(`/riesgos/${id}/evaluaciones`, datos).then((r) => r.data)
