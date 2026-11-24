import type { ClasificacionRiesgo } from '@/shared/riesgo/niveles'
import type { EstadoTarea } from '@/shared/tareas'

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

/** Investigación de un evento (HU-049). */
export interface Investigacion {
  causasInmediatas: string
  /** Causas de fondo. */
  causasBasicas: string
  conclusiones: string | null
  investigadaPorId: string
  investigadaPorNombre: string
  /** aaaa-mm-dd */
  fechaInvestigacion: string
}

/** GET /api/eventos/{id}: el evento con su investigación, cierre o descarte (HU-049). */
export interface EventoDetalle extends Evento {
  investigacion: Investigacion | null
  cerradoEn: string | null
  descartadoEn: string | null
  descartadoPorNombre: string | null
  motivoDescarte: string | null
  /** Medidas correctivas no canceladas (tareas del evento, HU-050); para cerrar hace falta al menos una (RN-28). */
  totalMedidas: number
}

/** PUT /api/eventos/{id}/investigacion */
export interface RegistrarInvestigacionRequest {
  causasInmediatas: string
  causasBasicas: string
  conclusiones?: string
  investigadaPorId: string
  fechaInvestigacion: string
}

/** Personas que pueden investigar o ser responsables de una medida: vínculos activos de la empresa, sin encuestados. */
export interface Persona {
  id: string
  nombreCompleto: string
  rolCodigo: string
}

/** GET y POST /api/eventos/{id}/medidas-correctivas (HU-050): una tarea del plan con origen en el evento. */
export interface MedidaCorrectiva {
  id: string
  titulo: string
  descripcion: string | null
  estado: EstadoTarea
  prioridad: 1 | 2 | 3
  responsableId: string | null
  responsableNombre: string | null
  /** aaaa-mm-dd */
  fechaLimite: string | null
  requiereEvidencia: boolean
  creadaEn: string
  completadaEn: string | null
}

/** POST /api/eventos/{id}/medidas-correctivas: nace asignada, como una tarea manual. */
export interface CrearMedidaRequest {
  titulo: string
  descripcion?: string
  prioridad: 1 | 2 | 3
  responsableId: string
  fechaLimite: string
  requiereEvidencia: boolean
}

/**
 * GET /api/eventos/{id}/riesgos (HU-051). `revelado`: el evento lo reveló (nivel = su nivel inicial actual).
 * `reevaluado`: ya estaba en la matriz y se reevaluó a raíz del evento (nivel anterior → nivel de esa evaluación).
 */
export interface RiesgoDelEvento {
  riesgoId: string
  nombre: string
  actividad: string | null
  relacion: 'revelado' | 'reevaluado'
  nivelAnterior: number | null
  clasificacionAnterior: ClasificacionRiesgo | null
  nivel: number | null
  clasificacion: ClasificacionRiesgo | null
  evaluadoEn: string | null
}

/** Lo que el evento necesita de un riesgo de la matriz (GET /api/riesgos) para elegir cuál reevaluar. */
export interface RiesgoDeMatriz {
  id: string
  peligroNombre: string | null
  descripcionPeligro: string | null
  actividad: string | null
  evaluacion: { nivel: number; clasificacion: ClasificacionRiesgo } | null
}

/** GET /api/eventos/estadisticas (HU-052): solo cifras, sin descartados ni datos de personas. */
export interface EstadisticasEventos {
  /** aaaa-mm-dd, incluidas. */
  desde: string
  hasta: string
  total: number
  graves: number
  /** Los cuatro tipos, en orden fijo, también con 0. */
  porTipo: { tipo: TipoEvento; cantidad: number; graves: number }[]
  /** Todos los meses del periodo (de Honduras), también con 0. */
  porMes: { anio: number; mes: number; cantidad: number; graves: number }[]
}
