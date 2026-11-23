import { useLocation } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import type { EstadoLogin } from '@/shared/sesion'

/**
 * Página de inicio de sesión. Provisional: el formulario se agrega con HU-018.
 * Si se llegó aquí porque la sesión venció, se explica antes de pedir los datos.
 */
export function LoginPage() {
  const { state } = useLocation()
  const motivo = (state as EstadoLogin | null)?.motivo

  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col gap-6 py-16">
      <h1 className="text-xl font-semibold">Iniciar sesión</h1>
      {motivo === 'expirada' && (
        <Aviso tipo="info">Tu sesión terminó. Vuelve a iniciar sesión para continuar.</Aviso>
      )}
      <p className="text-text-secondary">El formulario de inicio de sesión estará disponible pronto.</p>
    </section>
  )
}
