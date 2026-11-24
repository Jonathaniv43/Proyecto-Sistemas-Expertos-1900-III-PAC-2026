import { api } from '@/shared/api'
import type { CrearTareaManualRequest, PropuestaPlan, Responsable, Tarea } from '../types/plan'

/** Tareas que se agregarían al plan a partir de los riesgos y sus controles (HU-042). No guarda nada. */
export const obtenerPropuesta = (signal: AbortSignal) => api.get<PropuestaPlan>('/plan-trabajo/propuesta', { signal }).then((r) => r.data)

/** Confirma el plan con las tareas elegidas. Lo que ya tenía tarea no se repite. */
export const generarPlan = (claves: string[]) =>
  api.post<{ tareasCreadas: number }>('/plan-trabajo/generacion', { claves }).then((r) => r.data)

/** Tareas del plan, por prioridad y nivel del riesgo. */
export const listarTareas = (signal: AbortSignal) => api.get<Tarea[]>('/plan-trabajo/tareas', { signal }).then((r) => r.data)

/** A quién se le puede asignar una tarea (HU-043). */
export const listarResponsables = (signal: AbortSignal) =>
  api.get<Responsable[]>('/plan-trabajo/responsables', { signal }).then((r) => r.data)

/**
 * Asigna o reasigna responsable y fecha límite (HU-043). 400 `plan.responsable_invalido` o `plan.fecha_pasada`;
 * 409 `plan.tarea_cerrada` si ya está completada o cancelada.
 */
export const asignarTarea = (id: string, datos: { responsableId: string; fechaLimite: string }) =>
  api.put<Tarea>(`/plan-trabajo/tareas/${id}/asignacion`, datos).then((r) => r.data)

/** Crea una tarea manual, ya asignada (HU-043). */
export const crearTareaManual = (datos: CrearTareaManualRequest) => api.post<Tarea>('/plan-trabajo/tareas', datos).then((r) => r.data)
