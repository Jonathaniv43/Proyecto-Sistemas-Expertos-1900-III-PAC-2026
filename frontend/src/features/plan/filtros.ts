import { textoAIso } from '@/shared/validacion'
import type { FiltrosTareas } from './types/plan'

/** Valores del formulario de filtros del plan (HU-046), tal como se eligen en pantalla. */
export const FILTROS_VACIOS = { estado: '', responsable: '', origen: '', prioridad: '', desde: '', hasta: '' }
export type ValoresFiltros = typeof FILTROS_VACIOS

/** Fecha del filtro: vacía o a medio escribir no filtra; completa tiene que existir (31/02 no). */
export const fechaFiltro = (texto: string) => (texto.length === 10 ? textoAIso(texto) : null)
export const errorFecha = (texto: string) => (texto.length === 10 && !textoAIso(texto) ? 'Esa fecha no existe.' : undefined)

/** Lo elegido en el formulario, como parámetros de GET /plan-trabajo/tareas. Una fecha a medio escribir no filtra. */
export function aFiltros(v: ValoresFiltros): FiltrosTareas {
  const f: FiltrosTareas = {}
  if (v.estado === 'vencidas') f.vencidas = true
  else if (v.estado) f.estado = v.estado as FiltrosTareas['estado']
  if (v.responsable === 'sin_responsable') f.sinResponsable = true
  else if (v.responsable) f.responsableId = v.responsable
  if (v.origen) f.origen = v.origen as FiltrosTareas['origen']
  if (v.prioridad) f.prioridad = Number(v.prioridad) as 1 | 2 | 3
  const desde = fechaFiltro(v.desde)
  const hasta = fechaFiltro(v.hasta)
  if (desde && hasta && desde > hasta) return f // el error se muestra; no se filtra con un rango imposible
  if (desde) f.fechaDesde = desde
  if (hasta) f.fechaHasta = hasta
  return f
}
