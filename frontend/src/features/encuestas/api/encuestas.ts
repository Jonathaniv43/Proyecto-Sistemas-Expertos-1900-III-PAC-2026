import { api } from '@/shared/api'
import type {
  CategoriaDisponible,
  CrearEncuestaRequest,
  CrearEncuestaResponse,
  EncuestaDetalle,
  EncuestaPendiente,
  EncuestaResumen,
  GuardarBorradorRequest,
  ResponderEncuestaRequest,
  TipoRespuestaCatalogo,
  VersionEncuesta,
} from '../types/encuestas'

/** Categorías activas del rubro de la empresa de la sesión, con la vista previa de sus preguntas (HU-029). */
export const listarCategoriasDisponibles = (signal: AbortSignal) =>
  api.get<CategoriaDisponible[]>('/encuestas/categorias-disponibles', { signal }).then((r) => r.data)

/** Crea la encuesta como borrador con las categorías elegidas (HU-030). */
export const crearEncuesta = (datos: CrearEncuestaRequest) =>
  api.post<CrearEncuestaResponse>('/encuestas', datos).then((r) => r.data)

export const listarEncuestas = (signal: AbortSignal) =>
  api.get<EncuestaResumen[]>('/encuestas', { signal }).then((r) => r.data)

export const obtenerEncuesta = (signal: AbortSignal, id: string) =>
  api.get<EncuestaDetalle>(`/encuestas/${id}`, { signal }).then((r) => r.data)

/** Guarda el borrador. Si la encuesta está publicada y no tiene borrador, crea la versión siguiente. */
export const guardarBorrador = (id: string, datos: GuardarBorradorRequest) =>
  api.put<VersionEncuesta>(`/encuestas/${id}/borrador`, datos).then((r) => r.data)

/** Publica la versión en borrador; la publicada anterior pasa a reemplazada. */
export const publicarVersion = (id: string, versionId: string) =>
  api.post<VersionEncuesta>(`/encuestas/${id}/versiones/${versionId}/publicacion`).then((r) => r.data)

/** Cierra la encuesta: deja de recibir respuestas y ya no se puede editar. */
export const cerrarEncuesta = (id: string) => api.post(`/encuestas/${id}/cierre`)

export const listarTiposRespuesta = (signal: AbortSignal) =>
  api.get<TipoRespuestaCatalogo[]>('/encuestas/tipos-respuesta', { signal }).then((r) => r.data)

export const listarPendientes = (signal: AbortSignal) =>
  api.get<EncuestaPendiente[]>('/encuestas/pendientes', { signal }).then((r) => r.data)

/** Registra la respuesta anónima (HU-031): una sola vez por persona. */
export const responderEncuesta = (id: string, datos: ResponderEncuestaRequest) =>
  api.post<{ mensaje: string }>(`/encuestas/${id}/respuestas`, datos).then((r) => r.data)
