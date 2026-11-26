/** Estado de cuenta de la empresa en la plataforma (design-system.md § Badges de estado). */
export type EstadoEmpresa = 'activa' | 'suspendida' | 'inactiva'

/** GET /api/plataforma/empresas (HU-024): uso general de la plataforma por empresa, sin datos SST (RN-05). */
export interface EmpresaPlataformaResumen {
  empresaId: string
  razonSocial: string
  rubroNombre: string | null
  estado: EstadoEmpresa
  registradaEn: string
  vinculosActivos: number
}
