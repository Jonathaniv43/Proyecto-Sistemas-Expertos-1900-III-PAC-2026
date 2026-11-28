import { api } from '@/shared/api'
import { vencida, type EstadoTarea } from '@/shared/tareas'

/**
 * Lo que el inicio necesita de cada módulo (design-system.md § Inicio). Son llamadas propias a los endpoints que ya
 * existen, porque una feature no importa a otra; cada una devuelve solo las cifras que se muestran.
 */

interface TareaApi {
  estado: EstadoTarea
  fechaLimite: string | null
  responsableId: string | null
}

const porHacer = (t: TareaApi) => t.estado === 'pendiente' || t.estado === 'en_progreso'

export interface EncuestaPorResponder {
  encuestaId: string
  titulo: string
}

/** GET /api/encuestas/pendientes (HU-031): las que la persona todavía no respondió. */
export const obtenerEncuestasPorResponder = (signal: AbortSignal) =>
  api
    .get<EncuestaPorResponder[]>('/encuestas/pendientes', { signal })
    .then((r) => r.data.map(({ encuestaId, titulo }) => ({ encuestaId, titulo })))

/** GET /api/notificaciones/no-leidas (HU-117). */
export const contarNotificacionesSinLeer = (signal: AbortSignal) =>
  api.get<{ noLeidas: number }>('/notificaciones/no-leidas', { signal }).then((r) => r.data.noLeidas)

export interface ResumenMisTareas {
  porHacer: number
  vencidas: number
}

/** GET /api/plan-trabajo/tareas/mias (HU-044). "Vencida" es la misma regla que Mis tareas y el plan. */
export const obtenerMisTareas = (signal: AbortSignal) =>
  api.get<TareaApi[]>('/plan-trabajo/tareas/mias', { signal }).then((r): ResumenMisTareas => {
    const activas = r.data.filter(porHacer)
    return { porHacer: activas.length, vencidas: activas.filter(vencida).length }
  })

export interface ResumenPlan {
  vencidas: number
  sinResponsable: number
  /** Tareas que el plan propone y todavía no se confirmaron (HU-042). */
  propuestas: number
}

/** GET /api/plan-trabajo/tareas y /propuesta (HU-042, HU-043). */
export async function obtenerPlan(signal: AbortSignal): Promise<ResumenPlan> {
  const [tareas, propuesta] = await Promise.all([
    api.get<TareaApi[]>('/plan-trabajo/tareas', { signal }).then((r) => r.data),
    api.get<{ tareas: unknown[] }>('/plan-trabajo/propuesta', { signal }).then((r) => r.data),
  ])
  const activas = tareas.filter(porHacer)
  return {
    vencidas: activas.filter(vencida).length,
    sinResponsable: activas.filter((t) => t.responsableId === null).length,
    propuestas: propuesta.tareas.length,
  }
}

export interface ResumenMatriz {
  /** Altos o críticos sin controles (RN-19). */
  sinControl: number
  sinEvaluar: number
}

/** GET /api/riesgos/matriz (HU-038). */
export const obtenerMatriz = (signal: AbortSignal) =>
  api
    .get<{ altosSinControl: number; sinEvaluar: number }>('/riesgos/matriz', { signal })
    .then((r): ResumenMatriz => ({ sinControl: r.data.altosSinControl, sinEvaluar: r.data.sinEvaluar }))

export interface ResumenEventos {
  abiertos: number
  enInvestigacion: number
  /** Graves entre los abiertos y en investigación (HU-047). */
  graves: number
}

/** GET /api/eventos (HU-047, HU-049). */
export const obtenerEventos = (signal: AbortSignal) =>
  api.get<{ estado: string; esGrave: boolean }[]>('/eventos', { signal }).then((r): ResumenEventos => {
    const vivos = r.data.filter((e) => e.estado === 'abierto' || e.estado === 'en_investigacion')
    return {
      abiertos: vivos.filter((e) => e.estado === 'abierto').length,
      enInvestigacion: vivos.filter((e) => e.estado === 'en_investigacion').length,
      graves: vivos.filter((e) => e.esGrave).length,
    }
  })

/** GET /api/documentos (HU-070): cuántos están en borrador. */
export const contarDocumentosEnBorrador = (signal: AbortSignal) =>
  api.get<{ estado: string }[]>('/documentos', { signal }).then((r) => r.data.filter((d) => d.estado === 'borrador').length)

export interface ResumenPlataforma {
  empresas: number
  personas: number
  suspendidas: number
}

/** GET /api/plataforma/empresas (HU-024): sin datos SST (RN-05). */
export const obtenerResumenPlataforma = (signal: AbortSignal) =>
  api
    .get<{ estado: string; vinculosActivos: number }[]>('/plataforma/empresas', { signal })
    .then((r): ResumenPlataforma => ({
      empresas: r.data.length,
      personas: r.data.reduce((total, e) => total + e.vinculosActivos, 0),
      suspendidas: r.data.filter((e) => e.estado === 'suspendida').length,
    }))
