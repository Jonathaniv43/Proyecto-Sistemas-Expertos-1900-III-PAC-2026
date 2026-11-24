/** Catálogos globales de la plataforma (HU-023, D7). Los mantiene el staff de Norvia. */

export interface Rubro {
  id: number
  codigo: string
  nombre: string
  activo: boolean
}

export interface CategoriaRiesgo {
  id: string
  rubroId: number
  nombre: string
  descripcion: string | null
  activa: boolean
}

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

export interface Peligro {
  id: string
  codigo: string
  nombre: string
  descripcion: string | null
  clasificacion: ClasificacionPeligro
  activo: boolean
}

export type TipoRespuesta = 'respuesta_breve' | 'seleccion_unica' | 'seleccion_multiple'

export interface OpcionPregunta {
  id: string
  texto: string
  orden: number
  indicaExposicion: boolean
}

export interface Pregunta {
  id: string
  categoriaRiesgoId: string
  texto: string
  tipoRespuestaCodigo: TipoRespuesta
  obligatoria: boolean
  orden: number
  peligroId: string | null
  activa: boolean
  opciones: OpcionPregunta[]
}

/** Bloque de respuesta de una pregunta (D26): tipo, peligro y opciones van juntos. */
export interface BloqueRespuesta {
  tipoRespuestaCodigo: TipoRespuesta
  peligroId?: string
  opciones: { texto: string; orden: number; indicaExposicion: boolean }[]
}

export type TipoControl = 'eliminacion' | 'sustitucion' | 'ingenieria' | 'administrativo' | 'epp'

/** Con tipo, Norvia la sugiere como control (HU-039); sin tipo, es una tarea modelo del plan (HU-042). */
export interface PlantillaTarea {
  id: string
  peligroId: string | null
  tipoControl: TipoControl | null
  titulo: string
  descripcion: string | null
  prioridadSugerida: 1 | 2 | 3
  requiereEvidencia: boolean
  activa: boolean
}

export type TipoDocumento = 'politica' | 'procedimiento'

export interface PlantillaDocumento {
  id: string
  codigo: string
  titulo: string
  tipoDocumento: TipoDocumento
  /** null: sirve para todos los rubros. */
  rubroId: number | null
  contenido: string
  activa: boolean
}
