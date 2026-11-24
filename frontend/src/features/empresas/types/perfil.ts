/** GET y PATCH /api/empresas/perfil (HU-026). */
export interface PerfilEmpresa {
  empresaId: string
  razonSocial: string
  rtn: string
  direccion: string | null
  actividadEconomica: string | null
  rubroId: number | null
  /** El nombre del rubro es único en el catálogo: con él se marca la opción elegida. */
  rubroNombre: string | null
  trabajadoresPermanentes: number | null
  fechaInicioOperaciones: string | null
  estado: 'activa' | 'suspendida' | 'inactiva'
  /** true si ya tiene rubro y número de trabajadores. */
  perfilCompleto: boolean
}

/**
 * Edición parcial: se envía solo lo que cambia. RTN y estado no se editan.
 * Dirección y actividad vacías ("") se borran; rubro y trabajadores no se pueden borrar.
 */
export interface ActualizarPerfilRequest {
  razonSocial?: string
  direccion?: string
  actividadEconomica?: string
  rubroCodigo?: string
  trabajadoresPermanentes?: number
}
