import { api } from '@/shared/api'
import type { CategoriaDisponible, CrearEncuestaRequest, CrearEncuestaResponse } from '../types/encuestas'

/** Categorías activas del rubro de la empresa de la sesión, con la vista previa de sus preguntas (HU-029). */
export const listarCategoriasDisponibles = (signal: AbortSignal) =>
  api.get<CategoriaDisponible[]>('/encuestas/categorias-disponibles', { signal }).then((r) => r.data)

/** Crea la encuesta como borrador con las categorías elegidas (HU-030). */
export const crearEncuesta = (datos: CrearEncuestaRequest) =>
  api.post<CrearEncuestaResponse>('/encuestas', datos).then((r) => r.data)
