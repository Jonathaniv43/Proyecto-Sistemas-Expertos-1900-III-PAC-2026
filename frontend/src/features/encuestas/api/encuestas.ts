import { api } from '@/shared/api'
import type {
  CategoriaDisponible,
  ComparacionEncuestas,
  CrearEncuestaRequest,
  CrearEncuestaResponse,
  EncuestaDetalle,
  EncuestaHistorial,
  EncuestaPendiente,
  EncuestaResumen,
  GuardarBorradorRequest,
  PeligroRevisado,
  PeligrosSugeridos,
  ReporteEncuesta,
  ResponderEncuestaRequest,
  RevisarPeligroRequest,
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

export const obtenerReporte = (signal: AbortSignal, id: string, versionId?: string) =>
  api
    .get<ReporteEncuesta>(`/encuestas/${id}/reporte`, {
      signal,
      params: versionId ? { versionId } : undefined,
    })
    .then((r) => r.data)

/** HU-034: encuestas cerradas o con versiones anteriores (y vigentes si se piden para comparar). */
export const listarHistorialEncuestas = (
  signal: AbortSignal,
  filtros?: { buscar?: string; incluirVigentes?: boolean },
) =>
  api
    .get<EncuestaHistorial[]>('/encuestas/historial', {
      signal,
      params: {
        buscar: filtros?.buscar || undefined,
        incluirVigentes: filtros?.incluirVigentes || undefined,
      },
    })
    .then((r) => r.data)

/** HU-034: compara resultados y peligros sugeridos entre dos versiones publicadas o cerradas. */
export const compararEncuestas = (signal: AbortSignal, baseVersionId: string, compararVersionId: string) =>
  api
    .get<ComparacionEncuestas>('/encuestas/comparacion', {
      signal,
      params: { baseVersionId, compararVersionId },
    })
    .then((r) => r.data)

/** Peligros sugeridos por las respuestas (HU-033). Registra las sugerencias nuevas como riesgos sugeridos. */
export const obtenerPeligrosSugeridos = (signal: AbortSignal, id: string) =>
  api.get<PeligrosSugeridos>(`/encuestas/${id}/peligros-sugeridos`, { signal }).then((r) => r.data)

/** Confirma (pasa a la matriz) o descarta un peligro sugerido (HU-035). 409 si ya se revisó. */
export const revisarPeligroSugerido = (encuestaId: string, riesgoId: string, datos: RevisarPeligroRequest) =>
  api.patch<PeligroRevisado>(`/encuestas/${encuestaId}/peligros-sugeridos/${riesgoId}`, datos).then((r) => r.data)
