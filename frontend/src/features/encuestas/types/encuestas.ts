/** Tipos de respuesta de una pregunta (catálogo TipoRespuestaPregunta). */
export type TipoRespuesta = 'respuesta_breve' | 'seleccion_unica' | 'seleccion_multiple'

export interface OpcionPreview {
  texto: string
  orden: number
}

/** Pregunta tal como se incorporará a la encuesta (GET /api/encuestas/categorias-disponibles). */
export interface PreguntaPreview {
  id: string
  texto: string
  tipoRespuestaCodigo: TipoRespuesta | string
  /** Indicación para la persona según el tipo ("Elige una opción", etc.). */
  instruccionUi: string
  obligatoria: boolean
  orden: number
  opciones: OpcionPreview[]
}

export interface CategoriaDisponible {
  id: string
  nombre: string
  descripcion: string | null
  preguntasPreview: PreguntaPreview[]
}

export interface CrearEncuestaRequest {
  titulo: string
  instrucciones?: string
  categoriaRiesgoIds: string[]
}

export interface CrearEncuestaResponse {
  id: string
  titulo: string
  estado: 'borrador' | 'publicada' | 'cerrada'
  versionBorradorId: string
  versionPublicadaId: string | null
}
