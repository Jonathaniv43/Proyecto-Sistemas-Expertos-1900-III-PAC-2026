import type { ReactNode } from 'react'
import { CampanaNotificaciones } from '@/features/notificaciones/components/CampanaNotificaciones'
import type { UsuarioSesion } from '@/shared/api'
import { LayoutSesion } from '@/shared/components/layout'

/** LayoutSesion con las piezas de las features: la campana, solo en sesiones de empresa (HU-117). */
export function MarcoSesion({ usuario, children }: { usuario: UsuarioSesion; children: ReactNode }) {
  return (
    <LayoutSesion usuario={usuario} acciones={usuario.tipo === 'empresa' && <CampanaNotificaciones />}>
      {children}
    </LayoutSesion>
  )
}
