/** Tipo de evento (RN-26). */
export type TipoEvento = 'accidente' | 'incidente' | 'enfermedad_profesional' | 'condicion_insegura'

/** Estado del evento; investigar, cerrar y descartar llegan con HU-049. */
export type EstadoEvento = 'abierto' | 'en_investigacion' | 'cerrado' | 'descartado'

export type EstadoEnfermedad = 'sospechosa' | 'diagnosticada'

/** POST /api/eventos (HU-047): anónimo, no lleva quién reporta (RN-27, D11). */
export interface ReportarEventoRequest {
  tipo: TipoEvento
  estadoEnfermedad?: EstadoEnfermedad
  esGrave: boolean
  /** ISO con la zona de Honduras, p. ej. 2026-11-24T14:30:00-06:00. */
  fechaHoraEvento: string
  lugar?: string
  descripcion: string
}

/** GET /api/eventos y /api/eventos/{id}: sin dato alguno de quién lo reportó. */
export interface Evento {
  id: string
  tipo: TipoEvento
  estado: EstadoEvento
  estadoEnfermedad: EstadoEnfermedad | null
  esGrave: boolean
  fechaHoraEvento: string
  lugar: string | null
  descripcion: string
  /** Cuándo lo recibió el servidor. */
  registradoEn: string
}

export interface FiltrosEventos {
  tipo?: TipoEvento
  estado?: EstadoEvento
  esGrave?: boolean
}
