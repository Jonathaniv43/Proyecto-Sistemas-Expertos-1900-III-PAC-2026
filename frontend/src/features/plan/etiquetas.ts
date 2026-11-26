import { etiquetaRol } from '@/shared/sesion'
import type { Responsable, TipoControl } from './types/plan'

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

// Estados, prioridades y "vencida" se comparten con las medidas correctivas de eventos (HU-050).
export { ESTADOS_TAREA as ESTADOS, PRIORIDADES, vencida } from '@/shared/tareas'
