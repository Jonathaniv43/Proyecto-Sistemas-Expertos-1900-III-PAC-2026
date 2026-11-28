import { Link, useLocation } from 'react-router-dom'
import { itemActual, type GrupoMenu } from '@/shared/navegacion'

interface MenuNavegacionProps {
  grupos: readonly GrupoMenu[]
  /** Al elegir un destino (el panel de celular se cierra). */
  onNavegar?: () => void
}

/**
 * Destinos del menú del rol (design-system.md § Menú lateral). Marca el módulo de la pantalla actual: la propia
 * pantalla del menú (`aria-current="page"`) o una de sus pantallas internas (`aria-current="true"`).
 */
export function MenuNavegacion({ grupos, onNavegar }: MenuNavegacionProps) {
  const { pathname } = useLocation()
  const actual = itemActual(grupos, pathname)

  return (
    <nav aria-label="Menú principal" className="menu-nav">
      {grupos.map((grupo, i) => (
        <div key={grupo.titulo ?? i} className="menu-grupo">
          {grupo.titulo && (
            <span className="menu-grupo-titulo" id={`menu-grupo-${i}`}>
              {grupo.titulo}
            </span>
          )}
          <ul aria-labelledby={grupo.titulo ? `menu-grupo-${i}` : undefined}>
            {grupo.items.map(({ ruta, nombre, Icono }) => (
              <li key={ruta}>
                <Link
                  to={ruta}
                  className="item-menu"
                  aria-current={ruta === actual ? (ruta === pathname ? 'page' : 'true') : undefined}
                  onClick={onNavegar}
                >
                  <Icono aria-hidden size={20} strokeWidth={1.5} />
                  <span>{nombre}</span>
                </Link>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </nav>
  )
}
