import { api } from '@/shared/api'
import type {
  Control,
  ControlSugerido,
  CrearControlRequest,
  CrearRiesgoRequest,
  Evaluacion,
  FiltrosRiesgos,
  MatrizRiesgos,
  PeligroDisponible,
  Riesgo,
} from '../types/riesgos'

/** Riesgos con filtros (HU-036, HU-038): nivel más alto primero; sin estado, los de la matriz. */
export const listarRiesgos = (signal: AbortSignal, filtros: FiltrosRiesgos = {}) =>
  api.get<Riesgo[]>('/riesgos', { signal, params: filtros }).then((r) => r.data)

/** Cuántos riesgos de la matriz hay en cada celda de probabilidad × severidad (HU-038). */
export const obtenerMatriz = (signal: AbortSignal) => api.get<MatrizRiesgos>('/riesgos/matriz', { signal }).then((r) => r.data)

export const listarPeligrosDisponibles = (signal: AbortSignal) =>
  api.get<PeligroDisponible[]>('/riesgos/peligros-disponibles', { signal }).then((r) => r.data)

/** Agrega un riesgo a la matriz. 409 `riesgos.duplicado` si ya está con la misma actividad. */
export const crearRiesgo = (datos: CrearRiesgoRequest) => api.post<Riesgo>('/riesgos', datos).then((r) => r.data)

/** Un riesgo de la empresa, con su última evaluación. 404 si no existe o es de otra empresa. */
export const obtenerRiesgo = (signal: AbortSignal, id: string) => api.get<Riesgo>(`/riesgos/${id}`, { signal }).then((r) => r.data)

/** Evaluación inicial (HU-037). Se suma al historial: la anterior no se borra. 409 si el riesgo no está en la matriz. */
export const evaluarRiesgo = (id: string, datos: { probabilidad: number; severidad: number }) =>
  api.post<Evaluacion>(`/riesgos/${id}/evaluaciones`, datos).then((r) => r.data)

/** Controles del riesgo, del más efectivo al menos efectivo (HU-039). */
export const listarControles = (signal: AbortSignal, riesgoId: string) =>
  api.get<Control[]>(`/riesgos/${riesgoId}/controles`, { signal }).then((r) => r.data)

/** Controles que sugieren las plantillas del peligro y el riesgo todavía no aceptó (RN-76). */
export const listarControlesSugeridos = (signal: AbortSignal, riesgoId: string) =>
  api.get<ControlSugerido[]>(`/riesgos/${riesgoId}/controles/sugeridos`, { signal }).then((r) => r.data)

/**
 * Agrega un control propio o acepta una sugerencia (HU-039). 409 `riesgos.sugerencia_aceptada` si esa sugerencia
 * ya es un control; 400 `riesgos.sugerencia_invalida` si ya no está disponible; 409 `riesgos.no_en_matriz`.
 */
export const crearControl = (riesgoId: string, datos: CrearControlRequest) =>
  api.post<Control>(`/riesgos/${riesgoId}/controles`, datos).then((r) => r.data)
