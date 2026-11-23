/** Códigos de rol (catálogo Rol y RolStaffPlataforma del backend). */
export const ROLES = {
  adminEmpresa: 'admin_empresa',
  responsableSst: 'responsable_sst',
  colaborador: 'colaborador',
  encuestado: 'encuestado',
  adminPlataforma: 'admin_plataforma',
  soportePlataforma: 'soporte_plataforma',
} as const

export type Rol = (typeof ROLES)[keyof typeof ROLES]

/** Nombre para mostrar de cada rol (catálogo Rol / RolStaffPlataforma). */
export const ETIQUETAS_ROL: Record<Rol, string> = {
  admin_empresa: 'Administrador de empresa',
  responsable_sst: 'Responsable SST',
  colaborador: 'Colaborador',
  encuestado: 'Encuestado',
  admin_plataforma: 'Administrador de plataforma',
  soporte_plataforma: 'Soporte de plataforma',
}

export const etiquetaRol = (rol: string | null) => (rol && rol in ETIQUETAS_ROL ? ETIQUETAS_ROL[rol as Rol] : 'Sin rol')

/** Roles que gestionan la empresa (personas, encuestas, riesgos, plan). */
export const ROLES_GESTION: readonly Rol[] = [ROLES.adminEmpresa, ROLES.responsableSst]
