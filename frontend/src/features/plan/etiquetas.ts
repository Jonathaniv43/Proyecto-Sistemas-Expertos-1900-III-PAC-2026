import { etiquetaRol } from '@/shared/sesion'
import { hoyHonduras } from '@/shared/validacion'
import type { EstadoTarea, Responsable, Tarea, TipoControl } from './types/plan'

export const PRIORIDADES = { 1: 'Alta', 2: 'Media', 3: 'Baja' } as const

/** Opciones del select de responsable: nombre y rol, para distinguir homónimos. */
export const opcionesResponsable = (responsables: Responsable[]) =>
  responsables.map((r) => ({ valor: r.id, texto: r.nombreCompleto, descripcion: etiquetaRol(r.rolCodigo) }))

export const TIPOS_CONTROL: Record<TipoControl, string> = {
  eliminacion: 'Eliminación',
  sustitucion: 'Sustitución',
  ingenieria: 'Ingeniería',
  administrativo: 'Administrativo',
  epp: 'Equipo de protección personal',
}

/** Badge de cada estado (design-system.md § Badges de estado: Tarea del plan). */
export const ESTADOS: Record<EstadoTarea, { texto: string; tipo: 'neutro' | 'info' | 'exito' }> = {
  pendiente: { texto: 'Pendiente', tipo: 'neutro' },
  en_progreso: { texto: 'En progreso', tipo: 'info' },
  completada: { texto: 'Completada', tipo: 'exito' },
  cancelada: { texto: 'Cancelada', tipo: 'neutro' },
}

/** Pendiente o en progreso con la fecha límite pasada (no es un estado guardado). */
export const vencida = (t: Tarea) =>
  (t.estado === 'pendiente' || t.estado === 'en_progreso') && t.fechaLimite !== null && t.fechaLimite < hoyHonduras()
