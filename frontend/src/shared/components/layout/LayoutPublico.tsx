import type { ReactNode } from 'react'
import { Link, Outlet } from 'react-router-dom'

/**
 * Layout público (design-system.md § Layouts y navegación): landing, registro, login, activación, contraseña y acceso
 * del equipo. Una barra con "Norvia", que lleva a la landing, y el contenido en la columna de cada página.
 */
export function LayoutPublico({ children }: { children?: ReactNode }) {
  return (
    <>
      <header className="barra-publica">
        <div className="margen-lateral mx-auto flex h-full w-full max-w-layout-narrow items-center">
          <Link to="/" className="marca">
            Norvia
          </Link>
        </div>
      </header>
      <main className="pagina mx-auto w-full max-w-layout">{children ?? <Outlet />}</main>
    </>
  )
}
