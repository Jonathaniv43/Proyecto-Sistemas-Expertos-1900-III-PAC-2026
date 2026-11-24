import { api } from '@/shared/api'
import type {
  CrearMedidaRequest,
  Evento,
  EventoDetalle,
  FiltrosEventos,
  MedidaCorrectiva,
  Persona,
  RegistrarInvestigacionRequest,
  ReportarEventoRequest,
} from '../types/eventos'

/** Reporta un evento de forma anónima (HU-047). 400 `eventos.fecha_futura` si la fecha y hora todavía no llegan. */
export const reportarEvento = (datos: ReportarEventoRequest) => api.post<Evento>('/eventos', datos).then((r) => r.data)

/** Eventos de la empresa, el último recibido primero, con filtros (admin y Responsable SST). */
export const listarEventos = (signal: AbortSignal, filtros: FiltrosEventos = {}) =>
  api.get<Evento[]>('/eventos', { signal, params: filtros }).then((r) => r.data)

export const obtenerEvento = (signal: AbortSignal, id: string) => api.get<EventoDetalle>(`/eventos/${id}`, { signal }).then((r) => r.data)

/**
 * Registra o corrige la investigación (HU-049); el evento queda en investigación. 400 `eventos.investigador_invalido`
 * o `eventos.fecha_investigacion_invalida`; 409 `eventos.terminado`.
 */
export const registrarInvestigacion = (id: string, datos: RegistrarInvestigacionRequest) =>
  api.put<EventoDetalle>(`/eventos/${id}/investigacion`, datos).then((r) => r.data)

/** Cierra el evento. 409 `eventos.sin_investigacion` o `eventos.sin_medidas` (RN-28). */
export const cerrarEvento = (id: string) => api.post<EventoDetalle>(`/eventos/${id}/cierre`).then((r) => r.data)

/** Descarta un reporte falso, duplicado o inválido, con motivo opcional (RN-28). */
export const descartarEvento = (id: string, motivo: string) =>
  api.patch<EventoDetalle>(`/eventos/${id}/descarte`, { motivo: motivo.trim() || undefined }).then((r) => r.data)

/**
 * Quiénes pueden investigar o ser responsables de una medida: las mismas personas que pueden tener tareas del plan
 * (vínculos activos, sin encuestados).
 */
export const listarPersonas = (signal: AbortSignal) =>
  api.get<Persona[]>('/plan-trabajo/responsables', { signal }).then((r) => r.data)

/** Medidas correctivas del evento con su avance (HU-050): las abiertas primero, por fecha límite. */
export const listarMedidas = (signal: AbortSignal, id: string) =>
  api.get<MedidaCorrectiva[]>(`/eventos/${id}/medidas-correctivas`, { signal }).then((r) => r.data)

/**
 * Agrega una medida, que queda como tarea del plan con origen en el evento. 400 `eventos.responsable_invalido` o
 * `eventos.fecha_pasada`; 409 `eventos.terminado`.
 */
export const crearMedida = (id: string, datos: CrearMedidaRequest) =>
  api.post<MedidaCorrectiva>(`/eventos/${id}/medidas-correctivas`, datos).then((r) => r.data)
