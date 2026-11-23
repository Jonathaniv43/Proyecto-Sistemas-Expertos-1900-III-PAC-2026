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

/** Roles que gestionan la empresa (personas, encuestas, riesgos, plan). */
export const ROLES_GESTION: readonly Rol[] = [ROLES.adminEmpresa, ROLES.responsableSst]
