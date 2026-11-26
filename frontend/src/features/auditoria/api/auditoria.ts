import { api } from '@/shared/api'
import type { AuditoriaPaginada, FiltrosAuditoria } from '../types/auditoria'

export async function listarAuditoria(
  signal: AbortSignal,
  filtros: FiltrosAuditoria = {},
): Promise<AuditoriaPaginada> {
  const params: Record<string, string | number> = {}
  if (filtros.buscar?.trim()) params.buscar = filtros.buscar.trim()
  if (filtros.entidad) params.entidad = filtros.entidad
  if (filtros.accion) params.accion = filtros.accion
  if (filtros.desde) params.desde = filtros.desde
  if (filtros.hasta) params.hasta = filtros.hasta
  if (filtros.pagina && filtros.pagina > 1) params.pagina = filtros.pagina
  if (filtros.tamano && filtros.tamano !== 25) params.tamano = filtros.tamano

  const { data } = await api.get<AuditoriaPaginada>('/auditoria', { params, signal })
  return data
}
