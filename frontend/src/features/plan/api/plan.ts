import { api } from '@/shared/api'
import type { PropuestaPlan, Tarea } from '../types/plan'

/** Tareas que se agregarían al plan a partir de los riesgos y sus controles (HU-042). No guarda nada. */
export const obtenerPropuesta = (signal: AbortSignal) => api.get<PropuestaPlan>('/plan-trabajo/propuesta', { signal }).then((r) => r.data)

/** Confirma el plan con las tareas elegidas. Lo que ya tenía tarea no se repite. */
export const generarPlan = (claves: string[]) =>
  api.post<{ tareasCreadas: number }>('/plan-trabajo/generacion', { claves }).then((r) => r.data)

/** Tareas del plan, por prioridad y nivel del riesgo. */
export const listarTareas = (signal: AbortSignal) => api.get<Tarea[]>('/plan-trabajo/tareas', { signal }).then((r) => r.data)
