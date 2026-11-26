import type { ReactNode } from 'react'
import type { Rol } from './roles'
import { useTieneRol } from './useSesion'

/** Muestra `children` solo a los roles indicados (botones, ítems de menú, secciones). */
export function SiRol({ roles, children }: { roles: readonly Rol[]; children: ReactNode }) {
  return useTieneRol(roles) ? children : null
}
