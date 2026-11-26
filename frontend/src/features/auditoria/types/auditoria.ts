export type ActorAuditoriaTipo = 'empresa' | 'plataforma' | 'sistema'

export type EntidadAuditoria =
  | 'Documento'
  | 'VinculoEmpresa'
  | 'Encuesta'
  | 'Accidente'
  | 'RiesgoEmpresa'
  | 'EvaluacionRiesgo'
  | 'Control'
  | 'Tarea'
  | 'Empresa'

export interface RegistroAuditoria {
  id: string
  fecha: string
  accion: string
  entidad: EntidadAuditoria | string
  entidadId: string
  actorTipo: ActorAuditoriaTipo
  vinculoEmpresaId: string | null
  staffPlataformaId: string | null
  actorNombre: string
  actorCorreo: string | null
  detalle: Record<string, unknown> | null
}

export interface AuditoriaPaginada {
  items: RegistroAuditoria[]
  pagina: number
  tamano: number
  total: number
  totalPaginas: number
}

export interface FiltrosAuditoria {
  buscar?: string
  entidad?: string
  accion?: string
  desde?: string
  hasta?: string
  pagina?: number
  tamano?: number
}
