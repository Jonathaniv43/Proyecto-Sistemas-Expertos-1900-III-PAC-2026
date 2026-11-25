import type { EstadoVersionDocumento, TipoDocumento } from './types/documentos'

export const NOMBRE_TIPO: Record<TipoDocumento, string> = { politica: 'Política', procedimiento: 'Procedimiento' }

/** Badge de cada estado (design-system.md § Badges de estado: Política, también para procedimientos). */
export const ESTADOS_VERSION: Record<EstadoVersionDocumento, { texto: string; tipo: 'neutro' | 'exito' }> = {
  borrador: { texto: 'Borrador', tipo: 'neutro' },
  vigente: { texto: 'Vigente', tipo: 'exito' },
  reemplazada: { texto: 'Reemplazada', tipo: 'neutro' },
}

/** Fecha en Honduras, p. ej. "25 nov 2026". */
export const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { timeZone: 'America/Tegucigalpa', dateStyle: 'medium' })

