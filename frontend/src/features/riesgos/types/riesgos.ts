import type { ClasificacionRiesgo } from '@/shared/riesgo/niveles'

/** Clasificación del nivel de riesgo (RN-16); definida en shared porque también la usa el plan de trabajo. */
export type { ClasificacionRiesgo }

/** Estado de un riesgo de la empresa (RN-18). La matriz solo muestra los confirmados. */
export type EstadoRiesgo = 'sugerido' | 'confirmado' | 'descartado'

export type ClasificacionPeligro =
  | 'fisico'
  | 'mecanico'
  | 'electrico'
  | 'quimico'
  | 'biologico'
  | 'ergonomico'
  | 'psicosocial'
  | 'locativo'
  | 'otro'


/** Evaluación de un riesgo (HU-037). Nivel y clasificación los calcula el backend; no se guardan. */
export interface Evaluacion {
  id: string
  tipo: 'inicial' | 'residual'
  probabilidad: number
  severidad: number
  nivel: number
  clasificacion: ClasificacionRiesgo
  evaluadoPorId: string
  evaluadoPorNombre: string
  evaluadoEn: string
}

/** GET /api/riesgos y /api/riesgos/{id}, y respuesta de POST /api/riesgos (HU-036). */
export interface Riesgo {
  id: string
  estado: EstadoRiesgo
  peligroId: string | null
  /** Del catálogo; si es null, el peligro lo describió la empresa (`descripcionPeligro`). */
  peligroNombre: string | null
  /** Clasificación del peligro del catálogo; null si lo describió la empresa. */
  peligroClasificacion: ClasificacionPeligro | null
  descripcionPeligro: string | null
  actividad: string | null
  /** Encuesta que lo sugirió (HU-035); null si se agregó a mano. */
  encuestaId: string | null
  encuestaTitulo: string | null
  creadoEn: string
  /** Última evaluación inicial (HU-037); null si todavía no se evaluó. */
  evaluacion: Evaluacion | null
  /** Última evaluación residual, con los controles aplicados (HU-040); null si no hay. */
  evaluacionResidual: Evaluacion | null
  /** Cuántos controles tiene (HU-039). Un riesgo alto o crítico sin controles se señala (RN-19). */
  totalControles: number
}

/** GET /api/riesgos/peligros-disponibles: peligros activos del catálogo. */
export interface PeligroDisponible {
  id: string
  nombre: string
  descripcion: string | null
  clasificacion: ClasificacionPeligro
}

/** POST /api/riesgos: un peligro del catálogo o uno descrito (no los dos), y la actividad. */
export interface CrearRiesgoRequest {
  peligroId?: string
  descripcionPeligro?: string
  actividad: string
}

/** GET /api/riesgos/matriz (HU-038): solo las celdas con riesgos, por la última evaluación inicial. */
export interface MatrizRiesgos {
  total: number
  sinEvaluar: number
  /** Altos o críticos que todavía no tienen controles (RN-19, HU-039). */
  altosSinControl: number
  celdas: { probabilidad: number; severidad: number; nivel: number; clasificacion: ClasificacionRiesgo; cantidad: number }[]
}

/** Filtros de GET /api/riesgos (HU-038). Sin estado, los de la matriz (confirmados). */
export interface FiltrosRiesgos {
  estado?: EstadoRiesgo
  buscar?: string
  clasificacionPeligro?: ClasificacionPeligro
  nivel?: ClasificacionRiesgo
  sinEvaluar?: boolean
  /** Solo los altos o críticos sin controles (HU-039). */
  sinControl?: boolean
  probabilidad?: number
  severidad?: number
}

/** Jerarquía de controles (RN-21), del más efectivo al menos efectivo. */
export type TipoControl = 'eliminacion' | 'sustitucion' | 'ingenieria' | 'administrativo' | 'epp'

/** GET /api/riesgos/{id}/controles y respuesta de POST (HU-039). */
export interface Control {
  id: string
  riesgoId: string
  tipo: TipoControl
  descripcion: string
  estado: 'propuesto' | 'implementado'
  /** Plantilla del peligro de la que salió, si se aceptó una sugerencia (RN-76); null si es propio. */
  plantillaId: string | null
  creadoEn: string
}

/** GET /api/riesgos/{id}/controles/sugeridos: plantillas del peligro todavía sin aceptar. No se guardan hasta aceptarlas. */
export interface ControlSugerido {
  plantillaId: string
  tipo: TipoControl
  titulo: string
  descripcion: string | null
}

/** POST /api/riesgos/{id}/controles: propio o sugerencia aceptada (con `plantillaId`), tal cual o editada. */
export interface CrearControlRequest {
  tipo: TipoControl
  descripcion: string
  plantillaId?: string
}
