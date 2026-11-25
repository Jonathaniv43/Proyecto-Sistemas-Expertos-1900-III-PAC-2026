import { api } from '@/shared/api'
import type { DocumentoDetalle, DocumentoResumen, GenerarDocumentoRequest, PlantillaDisponible } from '../types/documentos'

/** Plantillas del rubro de la empresa (HU-070). 409 `documentos.sin_rubro` si el perfil no tiene rubro. */
export const listarPlantillas = (signal: AbortSignal) =>
  api.get<PlantillaDisponible[]>('/documentos/plantillas', { signal }).then((r) => r.data)

/**
 * Genera el borrador desde una plantilla (HU-070). 404 `documentos.plantilla_no_disponible` si la desactivaron o ya
 * no es del rubro; 409 `documentos.sin_rubro`.
 */
export const generarDocumento = (datos: GenerarDocumentoRequest) =>
  api.post<DocumentoDetalle>('/documentos/generacion', datos).then((r) => r.data)

export const listarDocumentos = (signal: AbortSignal) => api.get<DocumentoResumen[]>('/documentos', { signal }).then((r) => r.data)

export const obtenerDocumento = (signal: AbortSignal, id: string) =>
  api.get<DocumentoDetalle>(`/documentos/${id}`, { signal }).then((r) => r.data)
