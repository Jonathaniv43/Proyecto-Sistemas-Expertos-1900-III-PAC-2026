import { LayoutPublico } from '@/shared/components/layout'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { useSesion } from '@/shared/sesion'
import { MarcoSesion } from './MarcoSesion'
import { VerificandoSesion } from './PantallasSesion'

/**
 * Dirección inexistente: con sesión, dentro del layout de sesión (el menú sigue disponible); sin sesión, en el
 * layout público (design-system.md § Estados de pantalla).
 */
export function RutaNoEncontrada() {
  const sesion = useSesion()
  if (sesion.estado === 'verificando')
    return (
      <LayoutPublico>
        <VerificandoSesion />
      </LayoutPublico>
    )
  if (sesion.estado === 'activa')
    return (
      <MarcoSesion usuario={sesion.usuario}>
        <NotFoundPage />
      </MarcoSesion>
    )
  return (
    <LayoutPublico>
      <NotFoundPage />
    </LayoutPublico>
  )
}
