import { ROLES, useTieneRol } from '@/shared/sesion'

/** Solo el administrador de plataforma cambia los catálogos; soporte solo consulta (HU-023). */
export const usePuedeEditar = () => useTieneRol([ROLES.adminPlataforma])
