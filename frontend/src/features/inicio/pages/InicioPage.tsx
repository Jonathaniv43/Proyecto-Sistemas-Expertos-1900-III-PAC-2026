import { UserPlus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'
import { etiquetaRol, ROLES, SiRol, useSesion } from '@/shared/sesion'

/** Primera pantalla después de iniciar sesión. Las funciones de cada módulo se agregan con su HU. */
export function InicioPage() {
  const sesion = useSesion()
  if (sesion.estado !== 'activa') return null
  const { usuario } = sesion

  return (
    <section className="grid gap-6">
      <div className="grid gap-2">
        <h1 className="text-xl font-semibold">Inicio</h1>
        <p className="text-text-secondary">
          {usuario.tipo === 'plataforma'
            ? `Entraste a la administración de Norvia como ${etiquetaRol(usuario.rol)}.`
            : `Entraste${usuario.empresa ? ` a ${usuario.empresa.razonSocial}` : ''} como ${etiquetaRol(usuario.rol)}.`}
        </p>
      </div>
      <SiRol roles={[ROLES.adminEmpresa]}>
        <div>
          <Link to="/personas/nueva" className={clasesBoton('primario')}>
            <UserPlus aria-hidden size={16} strokeWidth={1.5} />
            Agregar una persona
          </Link>
        </div>
      </SiRol>
    </section>
  )
}
