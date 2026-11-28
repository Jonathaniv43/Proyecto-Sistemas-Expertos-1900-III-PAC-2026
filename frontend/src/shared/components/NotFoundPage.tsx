import { SearchX } from 'lucide-react'
import { useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useSesion } from '@/shared/sesion'
import { Volver } from './ui/Volver'

/**
 * Ruta inexistente o sin acceso: el mismo mensaje en ambos casos, para no confirmar que algo
 * existe en otra empresa o para otro rol (design-system.md § Estados de pantalla).
 * - Con sesión: dentro del layout de sesión, con "Ir al inicio" (/inicio). El menú sigue a la vista.
 * - Sin sesión: en el layout público, con "Volver" y el link a la página principal.
 */
export function NotFoundPage() {
  const conSesion = useSesion().estado === 'activa'
  const { pathname } = useLocation()
  // El título de la pestaña tampoco nombra la pantalla pedida (AppLayout lo pone antes desde la lista).
  useEffect(() => {
    document.title = 'Norvia'
  }, [pathname])

  const mensaje = (
    <section className="flex flex-col items-center gap-4 py-16 text-center">
      <SearchX aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
      <h1 className="text-md font-semibold">No encontramos lo que buscas</h1>
      <p className="text-text-secondary">La dirección no existe o no está disponible para tu cuenta.</p>
      {conSesion ? <Link to="/inicio">Ir al inicio</Link> : <Link to="/">Ir a la página principal</Link>}
    </section>
  )

  if (conSesion) return <div className="w-full max-w-layout">{mensaje}</div>
  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/" texto="Volver a la página principal" />
      {mensaje}
    </div>
  )
}
