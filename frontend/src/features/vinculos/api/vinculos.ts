import { api } from '@/shared/api'
import type {
  CrearEncuestadoRequest,
  CrearEncuestadoResponse,
  CrearEncuestadosLoteResponse,
  CrearVinculoRequest,
  CrearVinculoResponse,
  FilaEncuestado,
} from '../types/vinculos'

export const crearVinculo = (datos: CrearVinculoRequest) =>
  api.post<CrearVinculoResponse>('/vinculos', datos).then((r) => r.data)

export const crearEncuestado = (datos: CrearEncuestadoRequest) =>
  api.post<CrearEncuestadoResponse>('/vinculos/encuestados', datos).then((r) => r.data)

export const crearEncuestadosLote = (filas: FilaEncuestado[]) =>
  api.post<CrearEncuestadosLoteResponse>('/vinculos/encuestados/lote', { filas }).then((r) => r.data)
