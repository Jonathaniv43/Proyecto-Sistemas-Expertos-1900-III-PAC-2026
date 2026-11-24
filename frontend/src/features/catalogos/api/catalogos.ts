import { api } from '@/shared/api'
import type {
  BloqueRespuesta,
  CategoriaRiesgo,
  ClasificacionPeligro,
  Peligro,
  PlantillaDocumento,
  PlantillaTarea,
  Pregunta,
  Rubro,
  TipoControl,
  TipoDocumento,
} from '../types/catalogos'

/*
 * HU-023: catálogos de la plataforma (sesión de staff). Convenciones del backend:
 * - PATCH {id} es parcial: lo que no se envía se conserva; "" borra una descripción.
 * - PATCH {id}/estado { activo } activa o desactiva; no hay borrado.
 * - Errores: catalogos.codigo_duplicado / nombre_duplicado / orden_ocupado (409), *_invalido (400).
 */

const estado = (activo: boolean) => ({ activo })

// Rubros
export const listarRubros = (signal: AbortSignal) => api.get<Rubro[]>('/rubros', { signal }).then((r) => r.data)
export const crearRubro = (datos: { codigo: string; nombre: string }) => api.post<Rubro>('/rubros', datos).then((r) => r.data)
export const editarRubro = (id: number, datos: { nombre: string }) => api.patch<Rubro>(`/rubros/${id}`, datos).then((r) => r.data)
export const estadoRubro = (id: number, activo: boolean) => api.patch<Rubro>(`/rubros/${id}/estado`, estado(activo)).then((r) => r.data)

// Categorías de riesgo
export const listarCategorias = (signal: AbortSignal, rubroId?: number) =>
  api.get<CategoriaRiesgo[]>('/categorias-riesgo', { signal, params: { rubroId } }).then((r) => r.data)
export const crearCategoria = (datos: { rubroId: number; nombre: string; descripcion?: string }) =>
  api.post<CategoriaRiesgo>('/categorias-riesgo', datos).then((r) => r.data)
export const editarCategoria = (id: string, datos: { rubroId?: number; nombre?: string; descripcion?: string }) =>
  api.patch<CategoriaRiesgo>(`/categorias-riesgo/${id}`, datos).then((r) => r.data)
export const estadoCategoria = (id: string, activo: boolean) =>
  api.patch<CategoriaRiesgo>(`/categorias-riesgo/${id}/estado`, estado(activo)).then((r) => r.data)

// Peligros
export const listarPeligros = (signal: AbortSignal) => api.get<Peligro[]>('/peligros', { signal }).then((r) => r.data)
export const crearPeligro = (datos: { codigo: string; nombre: string; descripcion?: string; clasificacion: ClasificacionPeligro }) =>
  api.post<Peligro>('/peligros', datos).then((r) => r.data)
export const editarPeligro = (id: string, datos: { nombre?: string; descripcion?: string; clasificacion?: ClasificacionPeligro }) =>
  api.patch<Peligro>(`/peligros/${id}`, datos).then((r) => r.data)
export const estadoPeligro = (id: string, activo: boolean) =>
  api.patch<Peligro>(`/peligros/${id}/estado`, estado(activo)).then((r) => r.data)

// Preguntas exploratorias
export const listarPreguntas = (signal: AbortSignal, categoriaRiesgoId?: string) =>
  api.get<Pregunta[]>('/preguntas', { signal, params: { categoriaRiesgoId } }).then((r) => r.data)
export const crearPregunta = (datos: { categoriaRiesgoId: string; texto: string; obligatoria: boolean } & BloqueRespuesta) =>
  api.post<Pregunta>('/preguntas', datos).then((r) => r.data)
/** Para cambiar tipo, peligro u opciones se envía el bloque completo; las opciones se reemplazan. */
export const editarPregunta = (
  id: string,
  datos: { categoriaRiesgoId?: string; texto?: string; obligatoria?: boolean; orden?: number } & Partial<BloqueRespuesta>,
) => api.patch<Pregunta>(`/preguntas/${id}`, datos).then((r) => r.data)
export const estadoPregunta = (id: string, activo: boolean) =>
  api.patch<Pregunta>(`/preguntas/${id}/estado`, estado(activo)).then((r) => r.data)

// Plantillas de tarea
export const listarPlantillasTarea = (signal: AbortSignal, peligroId?: string) =>
  api.get<PlantillaTarea[]>('/plantillas-tarea', { signal, params: { peligroId } }).then((r) => r.data)
export interface DatosPlantillaTarea {
  tipoControl: TipoControl | null
  titulo: string
  descripcion: string
  prioridadSugerida: 1 | 2 | 3
  requiereEvidencia: boolean
}
export const crearPlantillaTarea = (peligroId: string, d: DatosPlantillaTarea) =>
  api.post<PlantillaTarea>('/plantillas-tarea', { peligroId, ...d, descripcion: d.descripcion || undefined }).then((r) => r.data)
export const editarPlantillaTarea = (id: string, d: DatosPlantillaTarea) =>
  api
    .patch<PlantillaTarea>(`/plantillas-tarea/${id}`, {
      tipoControl: d.tipoControl ?? undefined,
      quitarTipoControl: d.tipoControl === null ? true : undefined,
      titulo: d.titulo,
      descripcion: d.descripcion,
      prioridadSugerida: d.prioridadSugerida,
      requiereEvidencia: d.requiereEvidencia,
    })
    .then((r) => r.data)
export const estadoPlantillaTarea = (id: string, activo: boolean) =>
  api.patch<PlantillaTarea>(`/plantillas-tarea/${id}/estado`, estado(activo)).then((r) => r.data)

// Plantillas de documento (M9)
export const listarDocumentos = (signal: AbortSignal) => api.get<PlantillaDocumento[]>('/plantillas', { signal }).then((r) => r.data)
export const crearDocumento = (datos: { codigo: string; titulo: string; tipoDocumento: TipoDocumento; rubroId: number | null; contenido: string }) =>
  api.post<PlantillaDocumento>('/plantillas', datos).then((r) => r.data)
/** `rubroId: 0` = todos los rubros. */
export const editarDocumento = (id: string, datos: { titulo: string; tipoDocumento: TipoDocumento; rubroId: number; contenido: string }) =>
  api.patch<PlantillaDocumento>(`/plantillas/${id}`, datos).then((r) => r.data)
export const estadoDocumento = (id: string, activo: boolean) =>
  api.patch<PlantillaDocumento>(`/plantillas/${id}/estado`, estado(activo)).then((r) => r.data)
