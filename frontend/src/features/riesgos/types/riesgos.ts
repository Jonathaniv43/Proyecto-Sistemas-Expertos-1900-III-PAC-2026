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

/** GET /api/riesgos y /api/riesgos/{id}, y respuesta de POST /api/riesgos (HU-036). */
export interface Riesgo {
  id: string
  estado: EstadoRiesgo
  peligroId: string | null
  /** Del catálogo; si es null, el peligro lo describió la empresa (`descripcionPeligro`). */
  peligroNombre: string | null
  descripcionPeligro: string | null
  actividad: string | null
  /** Encuesta que lo sugirió (HU-035); null si se agregó a mano. */
  encuestaId: string | null
  encuestaTitulo: string | null
  creadoEn: string
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
