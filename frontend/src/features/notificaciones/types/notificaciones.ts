/** Origen de una notificación (D28). */
export type TipoNotificacion = 'tarea_asignada' | 'politica_publicada'

/** Una notificación de la bandeja (HU-117). */
export interface Notificacion {
  id: string
  tipo: TipoNotificacion
  titulo: string
  mensaje: string
  /** ISO con zona. */
  creadaEn: string
  /** Nula mientras no se lee. */
  leidaEn: string | null
  tareaId: string | null
  versionDocumentoId: string | null
  /** Documento de la política publicada: abre su versión vigente (HU-074). */
  documentoId: string | null
}

export interface MisNotificaciones {
  noLeidas: number
  notificaciones: Notificacion[]
}
