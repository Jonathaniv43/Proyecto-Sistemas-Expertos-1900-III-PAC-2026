import { hoyHonduras } from '@/shared/validacion'

/** Estado de una tarea del plan de trabajo (HU-044). */
export type EstadoTarea = 'pendiente' | 'en_progreso' | 'completada' | 'cancelada'

export const PRIORIDADES = { 1: 'Alta', 2: 'Media', 3: 'Baja' } as const

/** Badge de cada estado (design-system.md § Badges de estado: Tarea del plan). */
export const ESTADOS_TAREA: Record<EstadoTarea, { texto: string; tipo: 'neutro' | 'info' | 'exito' }> = {
  pendiente: { texto: 'Pendiente', tipo: 'neutro' },
  en_progreso: { texto: 'En progreso', tipo: 'info' },
  completada: { texto: 'Completada', tipo: 'exito' },
  cancelada: { texto: 'Cancelada', tipo: 'neutro' },
}

/** Pendiente o en progreso con la fecha límite pasada (no es un estado guardado). */
export const vencida = (t: { estado: EstadoTarea; fechaLimite: string | null }) =>
  (t.estado === 'pendiente' || t.estado === 'en_progreso') && t.fechaLimite !== null && t.fechaLimite < hoyHonduras()
