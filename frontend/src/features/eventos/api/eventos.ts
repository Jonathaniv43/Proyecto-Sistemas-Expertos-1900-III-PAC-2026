import { api } from '@/shared/api'
import type { Evento, FiltrosEventos, ReportarEventoRequest } from '../types/eventos'

/** Reporta un evento de forma anónima (HU-047). 400 `eventos.fecha_futura` si la fecha y hora todavía no llegan. */
export const reportarEvento = (datos: ReportarEventoRequest) => api.post<Evento>('/eventos', datos).then((r) => r.data)

/** Eventos de la empresa, el último recibido primero, con filtros (admin y Responsable SST). */
export const listarEventos = (signal: AbortSignal, filtros: FiltrosEventos = {}) =>
  api.get<Evento[]>('/eventos', { signal, params: filtros }).then((r) => r.data)

export const obtenerEvento = (signal: AbortSignal, id: string) => api.get<Evento>(`/eventos/${id}`, { signal }).then((r) => r.data)
