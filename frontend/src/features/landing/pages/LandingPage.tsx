import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'

/** Página de inicio pública: lleva al registro de la empresa (HU-017) o al inicio de sesión. */
export function LandingPage() {
  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col gap-6 py-16">
      <h1 className="text-2xl font-semibold tracking-tight">Norvia</h1>
      <p className="text-md text-text-secondary">
        Gestiona la seguridad y salud en el trabajo de tu empresa: identifica los riesgos, planifica las acciones y
        cumple el reglamento hondureño.
      </p>
      <div className="flex flex-col gap-3 sm:flex-row">
        <Link to="/registro" className={clasesBoton('primario')}>
          Registrar mi empresa
        </Link>
        <Link to="/login" className={clasesBoton('secundario')}>
          Iniciar sesión
        </Link>
      </div>
    </section>
  )
}
