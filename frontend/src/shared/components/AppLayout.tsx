import { useEffect } from 'react'
import { Outlet, ScrollRestoration, useLocation } from 'react-router-dom'
import { tituloPestana } from '@/shared/navegacion'

/**
 * Raíz de todas las pantallas: el fondo, el título de la pestaña ("{pantalla} · Norvia", de la lista única de
 * pantallas) y la posición de desplazamiento. Cada zona pone su layout: LayoutPublico o LayoutSesion
 * (design-system.md § Layouts y navegación).
 */
export function AppLayout() {
  return (
    <div className="min-h-dvh bg-bg-canvas">
      <TituloPestana />
      <Outlet />
      {/* Al entrar a una pantalla nueva arranca arriba; atrás/adelante recupera la posición. */}
      <ScrollRestoration />
    </div>
  )
}

/**
 * Va antes de <Outlet />: sus efectos corren antes que los de la pantalla, así una pantalla puede cambiar el título
 * después (el 404 pone "Norvia" para no confirmar que la ruta existe).
 */
function TituloPestana() {
  const { pathname } = useLocation()
  useEffect(() => {
    document.title = tituloPestana(pathname)
  }, [pathname])
  return null
}
