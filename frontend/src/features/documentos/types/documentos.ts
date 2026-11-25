/** Tipo de documento (D13). */
export type TipoDocumento = 'politica' | 'procedimiento'

/** Estado de una versión (RN-49 a RN-51): solo el borrador se edita. */
export type EstadoVersionDocumento = 'borrador' | 'vigente' | 'reemplazada'

/** GET /api/documentos/plantillas (HU-070): plantillas activas del rubro de la empresa o de todos los rubros. */
export interface PlantillaDisponible {
  id: string
  codigo: string
  titulo: string
  tipoDocumento: TipoDocumento
}

/** POST /api/documentos/generacion (HU-070). Sin título, toma el de la plantilla. */
export interface GenerarDocumentoRequest {
  plantillaDocumentoId: string
  titulo?: string
}

/** GET /api/documentos: estado y número de la versión de trabajo (el borrador si hay; si no, la vigente). */
export interface DocumentoResumen {
  id: string
  titulo: string
  tipoDocumento: TipoDocumento
  numeroVersion: number
  estado: EstadoVersionDocumento
  actualizadoEn: string
}

export interface VersionDocumento {
  id: string
  numeroVersion: number
  estado: EstadoVersionDocumento
  /** Markdown con los marcadores ya reemplazados. */
  contenido: string
  creadaEn: string
  creadaPorNombre: string
  aprobadaEn: string | null
  aprobadaPorNombre: string | null
}

/** GET /api/documentos/vigentes (HU-074): un documento vigente, para cualquier persona de la empresa. */
export interface DocumentoVigenteResumen {
  /** Id del documento. */
  id: string
  titulo: string
  tipoDocumento: TipoDocumento
  numeroVersion: number
  vigenteDesde: string
}

/** GET /api/documentos/vigentes/{id} (HU-074): solo lo que se publica de la versión vigente. */
export interface DocumentoVigente extends DocumentoVigenteResumen {
  /** Markdown, sin `[Completar: …]` (RN-49). */
  contenido: string
  aprobadaPorNombre: string
}

/** GET /api/documentos/{id} y respuesta de la generación. */
export interface DocumentoDetalle {
  id: string
  titulo: string
  tipoDocumento: TipoDocumento
  plantillaTitulo: string | null
  creadoEn: string
  version: VersionDocumento
  /** Lo que falta completar en la versión: el texto de cada `[Completar: …]` (RN-49). */
  porCompletar: string[]
}
