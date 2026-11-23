import { TriangleAlert } from 'lucide-react'
import { Boton } from '@/shared/components/ui/Boton'

/** Página para un error inesperado de la interfaz: nunca se muestra el detalle técnico. */
export function ErrorInesperado() {
  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col items-center gap-4 px-4 py-16 text-center">
      <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
      <h1 className="text-md font-semibold">Algo salió mal en esta pantalla</h1>
      <p className="text-text-secondary">Vuelve a cargarla. Si el problema sigue, avísale al equipo de Norvia.</p>
      <Boton variante="secundario" onClick={() => window.location.reload()}>
        Volver a cargar
      </Boton>
    </section>
  )
}
