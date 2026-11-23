import { ClipboardList, UserPlus, Users } from 'lucide-react'
import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'
import { etiquetaRol, ROLES, ROLES_GESTION, SiRol, useSesion } from '@/shared/sesion'

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
      <SiRol roles={ROLES_GESTION}>
        <div className="flex flex-col gap-3 sm:flex-row">
          <Link to="/encuestas/nueva" className={clasesBoton('secundario')}>
            <ClipboardList aria-hidden size={16} strokeWidth={1.5} />
            Crear una encuesta
          </Link>
        </div>
      </SiRol>
      <SiRol roles={[ROLES.adminEmpresa]}>
        <div className="flex flex-col gap-3 sm:flex-row">
          <Link to="/personas/nueva" className={clasesBoton('secundario')}>
            <UserPlus aria-hidden size={16} strokeWidth={1.5} />
            Agregar una persona
          </Link>
          <Link to="/personas/encuestados" className={clasesBoton('secundario')}>
            <Users aria-hidden size={16} strokeWidth={1.5} />
            Agregar encuestados
          </Link>
        </div>
      </SiRol>
    </section>
  )
}
