import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col items-center gap-4 py-16 text-center">
      <h1 className="text-xl font-semibold">No encontramos esta página</h1>
      <p className="text-text-secondary">La dirección no existe o ya no está disponible.</p>
      <Link to="/">Volver al inicio</Link>
    </section>
  )
}
