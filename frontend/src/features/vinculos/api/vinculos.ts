import { api } from '@/shared/api'
import type {
  ActualizarVinculoRequest,
  CrearEncuestadoRequest,
  CrearEncuestadoResponse,
  CrearEncuestadosLoteResponse,
  CrearVinculoRequest,
  CrearVinculoResponse,
  FilaEncuestado,
  Vinculo,
} from '../types/vinculos'

export const crearVinculo = (datos: CrearVinculoRequest) =>
  api.post<CrearVinculoResponse>('/vinculos', datos).then((r) => r.data)

export const crearEncuestado = (datos: CrearEncuestadoRequest) =>
  api.post<CrearEncuestadoResponse>('/vinculos/encuestados', datos).then((r) => r.data)

export const crearEncuestadosLote = (filas: FilaEncuestado[]) =>
  api.post<CrearEncuestadosLoteResponse>('/vinculos/encuestados/lote', { filas }).then((r) => r.data)

/** Personas con acceso a la empresa de la sesión, ordenadas por apellido (HU-021). */
export const listarVinculos = (signal: AbortSignal) => api.get<Vinculo[]>('/vinculos', { signal }).then((r) => r.data)

/** Cambia el rol o el estado del acceso; suspender o dar de baja cierra las sesiones de esa persona en esta empresa. */
export const actualizarVinculo = (id: string, datos: ActualizarVinculoRequest) =>
  api.patch<Vinculo>(`/vinculos/${id}`, datos).then((r) => r.data)
