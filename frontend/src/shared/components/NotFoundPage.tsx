import { Link } from 'react-router-dom'

/**
 * Ruta inexistente o sin acceso: el mismo mensaje en ambos casos, para no confirmar que algo
 * existe en otra empresa o para otro rol (design-system.md § Estados de pantalla).
 */
export function NotFoundPage() {
  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col items-center gap-4 py-16 text-center">
      <h1 className="text-md font-semibold">No encontramos lo que buscas</h1>
      <p className="text-text-secondary">La dirección no existe o no está disponible para tu cuenta.</p>
      <Link to="/">Volver al inicio</Link>
    </section>
  )
}
