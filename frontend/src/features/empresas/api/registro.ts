import { api } from '@/shared/api'
import type {
  RegistrarEmpresaMicrosoftRequest,
  RegistrarEmpresaRequest,
  RegistrarEmpresaResponse,
  RubroRegistro,
} from '../types/registro'

export const listarRubrosRegistro = (signal: AbortSignal) =>
  api.get<RubroRegistro[]>('/empresas/rubros-registro', { signal }).then((r) => r.data)

export const registrarEmpresa = (datos: RegistrarEmpresaRequest) =>
  api.post<RegistrarEmpresaResponse>('/empresas', datos).then((r) => r.data)

export const registrarEmpresaMicrosoft = (datos: RegistrarEmpresaMicrosoftRequest) =>
  api.post<RegistrarEmpresaResponse>('/empresas/microsoft', datos).then((r) => r.data)
