import { Link } from 'react-router-dom'
import { Volver } from './ui/Volver'

/**
 * Ruta inexistente o sin acceso: el mismo mensaje en ambos casos, para no confirmar que algo
 * existe en otra empresa o para otro rol (design-system.md § Estados de pantalla).
 */
export function NotFoundPage() {
  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/" texto="Volver a la página principal" />
      <section className="flex flex-col items-center gap-4 py-16 text-center">
        <h1 className="text-md font-semibold">No encontramos lo que buscas</h1>
        <p className="text-text-secondary">La dirección no existe o no está disponible para tu cuenta.</p>
        <Link to="/">Ir a la página principal</Link>
      </section>
    </div>
  )
}
