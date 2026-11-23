/** Tipos de respuesta de una pregunta (catálogo TipoRespuestaPregunta). */
export type TipoRespuesta = 'respuesta_breve' | 'seleccion_unica' | 'seleccion_multiple'

export interface OpcionPreview {
  /** Solo al responder (HU-031): el id que se envía como respuesta. */
  id?: string
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

export type EstadoEncuesta = 'borrador' | 'publicada' | 'cerrada'
export type EstadoVersion = 'borrador' | 'publicada' | 'reemplazada'

/** GET /api/encuestas */
export interface EncuestaResumen {
  id: string
  titulo: string
  estado: EstadoEncuesta
  creadaEn: string
  versionPublicadaNumero: number | null
  participaciones: number
}

export interface OpcionEncuesta {
  id: string
  texto: string
  orden: number
  indicaExposicion: boolean
}

export interface PreguntaEncuestaVersion {
  id: string
  seccionId: string | null
  categoriaRiesgoId: string | null
  preguntaExploratoriaId: string | null
  texto: string
  tipoRespuestaCodigo: TipoRespuesta | string
  instruccionUi: string
  obligatoria: boolean
  peligroId: string | null
  orden: number
  opciones: OpcionEncuesta[]
}

export interface SeccionEncuesta {
  id: string
  titulo: string
  orden: number
}

export interface VersionEncuesta {
  id: string
  numeroVersion: number
  estado: EstadoVersion
  instrucciones: string | null
  publicadaEn: string | null
  secciones: SeccionEncuesta[]
  preguntas: PreguntaEncuestaVersion[]
}

/** GET /api/encuestas/{id} */
export interface EncuestaDetalle {
  id: string
  titulo: string
  estado: EstadoEncuesta
  /** VinculoEmpresa de quien la creó: solo esa persona puede editarla, publicarla o cerrarla. */
  creadaPorId: string
  creadaEn: string
  cerradaEn: string | null
  versionPublicada: VersionEncuesta | null
  versionBorrador: VersionEncuesta | null
}

/** PUT /api/encuestas/{id}/borrador: el estado completo del borrador (secciones + preguntas). */
export interface GuardarBorradorRequest {
  titulo: string
  instrucciones: string | null
  secciones: { id: string; titulo: string; orden: number }[]
  preguntas: {
    id: string | null
    seccionId: string | null
    categoriaRiesgoId: string | null
    texto: string
    tipoRespuestaCodigo: TipoRespuesta
    obligatoria: boolean
    peligroId: string | null
    orden: number
    opciones: { id: string | null; texto: string; orden: number; indicaExposicion: boolean }[]
  }[]
}

/** GET /api/encuestas/tipos-respuesta */
export interface TipoRespuestaCatalogo {
  id: string
  codigo: TipoRespuesta | string
  nombre: string
  instruccionUi: string
  requiereOpciones: boolean
  maxCaracteresRespuesta: number | null
}

/** GET /api/encuestas/pendientes: encuestas publicadas que la persona de la sesión aún no respondió. */
export interface EncuestaPendiente {
  encuestaId: string
  titulo: string
  /** Versión vigente: la respuesta se registra contra ella. */
  versionId: string
  instrucciones: string | null
  preguntas: {
    id: string
    texto: string
    tipoRespuesta: TipoRespuesta | string
    instruccionUi: string
    obligatoria: boolean
    maxCaracteres: number | null
    opciones: { id: string; texto: string }[]
  }[]
}

/** POST /api/encuestas/{id}/respuestas: cada respuesta lleva exactamente uno de opcionId, textoLibre u opcionIds. */
export interface ResponderEncuestaRequest {
  versionId: string
  respuestas: { preguntaId: string; opcionId?: string; textoLibre?: string; opcionIds?: string[] }[]
}
