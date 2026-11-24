import { Outlet, ScrollRestoration } from 'react-router-dom'

/**
 * Contenedor general: ancho máximo y espacios de seguridad (`.pagina`): margen lateral de 16/24/32px
 * (celular/tablet/escritorio), aire arriba y abajo, y el área segura del dispositivo. Se configuran en
 * theme.css (--layout-gutter, --layout-space-top, --layout-space-bottom).
 */
export function AppLayout() {
  return (
    <div className="min-h-dvh bg-bg-canvas">
      <main className="pagina mx-auto w-full max-w-layout">
        <Outlet />
      </main>
      {/* Al entrar a una pantalla nueva arranca arriba; atrás/adelante recupera la posición. */}
      <ScrollRestoration />
    </div>
  )
}
