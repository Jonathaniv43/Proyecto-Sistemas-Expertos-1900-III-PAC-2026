import { WifiOff } from 'lucide-react'
import { Boton } from '@/shared/components/ui/Boton'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { restaurarSesion } from '@/shared/sesion'

/** Mientras se recupera la sesión al recargar: skeleton solo si tarda más de ~300ms. */
export function VerificandoSesion() {
  const visible = useCargaVisible(true)
  if (!visible) return null
  return (
    <div aria-busy="true" aria-label="Cargando" className="grid gap-4 py-8">
      <div className="h-control w-1/3 animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
      <div className="h-table-row w-full animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
      <div className="h-table-row w-2/3 animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
    </div>
  )
}

/** Hay sesión guardada pero el backend no respondió: no se cierra la sesión, se ofrece reintentar. */
export function SesionSinConexion() {
  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col items-center gap-4 py-16 text-center">
      <WifiOff aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
      <h1 className="text-md font-semibold">No se pudo conectar con Norvia</h1>
      <p className="text-text-secondary">Revisa tu conexión e inténtalo de nuevo.</p>
      <Boton variante="secundario" onClick={() => void restaurarSesion()}>
        Reintentar
      </Boton>
    </section>
  )
}
