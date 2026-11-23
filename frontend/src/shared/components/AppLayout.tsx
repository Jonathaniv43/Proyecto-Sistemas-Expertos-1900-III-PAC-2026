import { Outlet } from 'react-router-dom'

/**
 * Contenedor general: ancho máximo y márgenes laterales del layout
 * (16px en celular, 24px en tablet, 32px en escritorio).
 */
export function AppLayout() {
  return (
    <div className="min-h-dvh bg-bg-canvas">
      <main className="mx-auto w-full max-w-layout px-4 py-8 sm:px-6 lg:px-8">
        <Outlet />
      </main>
    </div>
  )
}
