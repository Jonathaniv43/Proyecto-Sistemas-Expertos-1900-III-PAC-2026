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

/** Roles que se pueden asignar a una persona dentro de una empresa. */
export const ROLES_EMPRESA = [ROLES.adminEmpresa, ROLES.responsableSst, ROLES.colaborador, ROLES.encuestado] as const

/** Qué puede hacer cada rol de empresa, en lenguaje de la persona (catálogo de permisos del backend). */
export const DESCRIPCIONES_ROL_EMPRESA: Record<(typeof ROLES_EMPRESA)[number], string> = {
  admin_empresa: 'Administra la empresa en Norvia: agrega personas y gestiona su acceso.',
  responsable_sst: 'Gestiona la seguridad y salud: crea encuestas, revisa sus resultados y los eventos reportados.',
  colaborador: 'Responde encuestas, reporta eventos y atiende las tareas que se le asignen.',
  encuestado: 'Responde encuestas, consulta las políticas vigentes y reporta eventos de forma anónima.',
}

/** Roles que gestionan la empresa (personas, encuestas, riesgos, plan). */
export const ROLES_GESTION: readonly Rol[] = [ROLES.adminEmpresa, ROLES.responsableSst]
