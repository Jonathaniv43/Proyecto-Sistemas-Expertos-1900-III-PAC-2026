import { etiquetaRol } from '@/shared/sesion'
import type { Responsable } from './types/plan'

export const PRIORIDADES = { 1: 'Alta', 2: 'Media', 3: 'Baja' } as const

/** Opciones del select de responsable: nombre y rol, para distinguir homónimos. */
export const opcionesResponsable = (responsables: Responsable[]) =>
  responsables.map((r) => ({ valor: r.id, texto: r.nombreCompleto, descripcion: etiquetaRol(r.rolCodigo) }))
