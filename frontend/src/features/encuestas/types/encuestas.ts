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

/** Resumen de una versión publicada o reemplazada de una encuesta (HU-030, HU-034). */
export interface VersionEncuestaResumen {
  id: string
  numeroVersion: number
  estado: EstadoVersion
  publicadaEn: string | null
  totalRespuestasAnonimas: number
  categorias: string[]
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
  versionesPublicadas: VersionEncuestaResumen[]
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

/** GET /api/encuestas/{id}/reporte (HU-032, HU-034): resultados anónimos de la versión indicada o vigente. */
export interface ReporteEncuesta {
  encuestaId: string
  /** Personas que respondieron la encuesta (todas sus versiones). */
  totalParticipaciones: number
  /** Respuestas registradas en la versión consultada. */
  totalRespuestasAnonimas: number
  periodoDesde: string | null
  periodoHasta: string | null
  /** Con menos de 3 respuestas no se muestra el detalle, para no identificar a nadie (RN-73). */
  detalleOculto: boolean
  preguntas: {
    preguntaId: string
    texto: string
    tipoRespuesta: TipoRespuesta | string
    /** Selección: cantidad y porcentaje sobre quienes respondieron esa pregunta. */
    distribucion: { opcionId: string; etiqueta: string; cantidad: number; porcentaje: number }[] | null
    /** Respuesta breve: los textos, sin autor. */
    textosBreves: string[] | null
  }[]
  versionId: string
  numeroVersion: number
  estadoVersion: EstadoVersion
}

/** GET /api/encuestas/historial (HU-034) */
export interface EncuestaHistorial {
  id: string
  titulo: string
  estado: EstadoEncuesta
  creadaEn: string
  cerradaEn: string | null
  ultimaPublicacionEn: string | null
  categorias: string[]
  resumen: {
    participaciones: number
    respuestasAnonimas: number
  }
  versiones: VersionEncuestaResumen[]
}

export interface LadoComparacion {
  encuestaId: string
  titulo: string
  estadoEncuesta: EstadoEncuesta
  versionId: string
  numeroVersion: number
  estadoVersion: EstadoVersion
  publicadaEn: string | null
  cerradaEn: string | null
  participaciones: number
  totalRespuestasAnonimas: number
  periodoDesde: string | null
  periodoHasta: string | null
  detalleOculto: boolean
  categorias: string[]
  cantidadRespuestasBreves: number
}

export interface OpcionComparada {
  etiqueta: string
  cantidadBase: number
  porcentajeBase: number
  cantidadComparar: number
  porcentajeComparar: number
  diferenciaPuntosPorcentuales: number
}

export interface PreguntaComparada {
  claveEmparejamiento: 'catalogo' | 'texto' | string
  textoBase: string
  textoComparar: string
  tipoRespuesta: TipoRespuesta | string
  respondieronBase: number
  respondieronComparar: number
  opciones: OpcionComparada[]
}

export interface PeligroComparado {
  peligroId: string
  peligroNombre: string
  respuestasBase: number
  porcentajeBase: number
  estadoEnEncuestaBase: EstadoRiesgo | null
  respuestasComparar: number
  porcentajeComparar: number
  estadoEnEncuestaComparar: EstadoRiesgo | null
  diferenciaPuntosPorcentuales: number
}

/** GET /api/encuestas/comparacion (HU-034) */
export interface ComparacionEncuestas {
  base: LadoComparacion
  comparar: LadoComparacion
  minimoRespuestas: number
  comparacionOculta: boolean
  preguntasComparadas: PreguntaComparada[]
  preguntasSoloBase: ReporteEncuesta['preguntas']
  preguntasSoloComparar: ReporteEncuesta['preguntas']
  peligrosComparados: PeligroComparado[]
}

/** Estado del riesgo que registra cada sugerencia (RN-18). Se confirma o descarta en HU-035. */
export type EstadoRiesgo = 'sugerido' | 'confirmado' | 'descartado'

/** Peligros sugeridos por las respuestas (HU-033, RN-13). Se calculan con todas las versiones. */
export interface PeligrosSugeridos {
  encuestaId: string
  /** Respuestas de todas las versiones: base del respaldo de cada sugerencia. */
  totalRespuestas: number
  minimoRespuestas: number
  /** Con menos de `minimoRespuestas` no se sugiere nada, para no identificar a nadie (RN-73). */
  detalleOculto: boolean
  /** Ordenadas por respaldo, de más a menos. */
  sugerencias: {
    riesgoSugeridoId: string
    peligroId: string
    peligroNombre: string
    estado: EstadoRiesgo
    /** Actividad donde se presenta, si se indicó al confirmarlo (HU-035). */
    actividad: string | null
    /** Respuestas que eligieron una opción de exposición; cada una cuenta una vez por peligro. */
    respuestasConExposicion: number
    preguntas: { preguntaId: string; texto: string; respuestasConExposicion: number }[]
  }[]
}

/** PATCH /api/encuestas/{id}/peligros-sugeridos/{riesgoId} (HU-035): se revisa una sola vez. */
export interface RevisarPeligroRequest {
  estado: 'confirmado' | 'descartado'
  /** Solo al confirmar: actividad o lugar donde se presenta el riesgo. */
  actividad?: string
}

export interface PeligroRevisado {
  riesgoId: string
  encuestaId: string
  peligroId: string
  estado: EstadoRiesgo
  actividad: string | null
  revisadoPorId: string
  revisadoEn: string
}
