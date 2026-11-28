import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { clasesBoton } from '@/shared/components/ui/clases'
import { etiquetaRol, useSesion } from '@/shared/sesion'

/**
 * Página de inicio pública: lleva al registro de la empresa (HU-017) o al inicio de sesión (busca por RTN).
 * Con sesión iniciada lo dice y ofrece ir al inicio, en lugar de registrar o iniciar sesión otra vez
 * (design-system.md § Layouts y navegación).
 */
export function LandingPage() {
  const sesion = useSesion()
  const usuario = sesion.estado === 'activa' ? sesion.usuario : null
  const lugar = usuario?.tipo === 'plataforma' ? 'la administración de Norvia' : (usuario?.empresa?.razonSocial ?? 'tu empresa')

  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col gap-6">
      <h1 className="text-2xl font-semibold tracking-tight">Norvia</h1>
      <p className="text-md text-text-secondary">
        Gestiona la seguridad y salud en el trabajo de tu empresa: identifica los riesgos, planifica las acciones y
        cumple el reglamento hondureño.
      </p>
      {usuario ? (
        <>
          <Aviso tipo="info">
            Ya entraste a {lugar} como {etiquetaRol(usuario.rol)}.
          </Aviso>
          <div className="flex flex-col gap-3 sm:flex-row sm:self-start">
            <Link to="/inicio" className={clasesBoton('primario')}>
              Ir a mi inicio
            </Link>
          </div>
          <p className="text-text-secondary">Para entrar a otra empresa, cierra la sesión desde el menú de tu cuenta.</p>
        </>
      ) : (
        <>
          <div className="flex flex-col gap-3 sm:flex-row sm:self-start">
            <Link to="/registro" className={clasesBoton('primario')}>
              Registrar mi empresa
            </Link>
          </div>
          <div className="grid gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:flex sm:items-center sm:justify-between sm:p-6">
            <div className="grid gap-1">
              <h2 className="text-md font-semibold">¿Ya estás registrado?</h2>
              <p className="text-text-secondary">Busca tu empresa por su RTN e inicia sesión.</p>
            </div>
            <Link to="/login" className={clasesBoton('secundario')}>
              Iniciar sesión
            </Link>
          </div>
        </>
      )}
    </section>
  )
}
