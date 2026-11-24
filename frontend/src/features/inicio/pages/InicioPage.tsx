import { BookOpen, Building2, Megaphone, Siren, ClipboardCheck, ClipboardList, Contact, ListChecks, ListTodo, ShieldAlert, UserPlus, Users } from 'lucide-react'
import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { clasesBoton } from '@/shared/components/ui/clases'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { etiquetaRol, ROLES, ROLES_EMPRESA, ROLES_GESTION, ROLES_PLATAFORMA, ROLES_TAREAS, SiRol, useSesion, useTieneRol } from '@/shared/sesion'
import { obtenerPerfilCompleto } from '../api/perfil'

/** Primera pantalla después de iniciar sesión. Las funciones de cada módulo se agregan con su HU. */
export function InicioPage() {
  const sesion = useSesion()
  const administra = useTieneRol([ROLES.adminEmpresa])
  const perfil = usePeticion(obtenerPerfilCompleto)
  const { ejecutar: cargarPerfil } = perfil
  useEffect(() => {
    if (administra) void cargarPerfil()
  }, [administra, cargarPerfil])

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
      {administra && perfil.datos === false && (
        <Aviso tipo="advertencia" titulo="Completa el perfil de la empresa">
          Falta el rubro o el número de trabajadores permanentes. <Link to="/empresa">Completar el perfil</Link>
        </Aviso>
      )}
      <SiRol roles={ROLES_PLATAFORMA}>
        <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Link to="/catalogos" className={clasesBoton('secundario')}>
            <BookOpen aria-hidden size={16} strokeWidth={1.5} />
            Catálogos
          </Link>
        </div>
      </SiRol>
      <SiRol roles={ROLES_EMPRESA}>
        <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Link to="/mis-encuestas" className={clasesBoton('secundario')}>
            <ClipboardCheck aria-hidden size={16} strokeWidth={1.5} />
            Mis encuestas
          </Link>
          <Link to="/eventos/reportar" className={clasesBoton('secundario')}>
            <Megaphone aria-hidden size={16} strokeWidth={1.5} />
            Reportar un evento
          </Link>
          <SiRol roles={ROLES_TAREAS}>
            <Link to="/mis-tareas" className={clasesBoton('secundario')}>
              <ListTodo aria-hidden size={16} strokeWidth={1.5} />
              Mis tareas
            </Link>
          </SiRol>
        </div>
      </SiRol>
      <SiRol roles={ROLES_GESTION}>
        <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Link to="/encuestas" className={clasesBoton('secundario')}>
            <ClipboardList aria-hidden size={16} strokeWidth={1.5} />
            Encuestas
          </Link>
          <Link to="/riesgos" className={clasesBoton('secundario')}>
            <ShieldAlert aria-hidden size={16} strokeWidth={1.5} />
            Matriz de riesgos
          </Link>
          <Link to="/eventos" className={clasesBoton('secundario')}>
            <Siren aria-hidden size={16} strokeWidth={1.5} />
            Eventos
          </Link>
          <Link to="/plan-de-trabajo" className={clasesBoton('secundario')}>
            <ListChecks aria-hidden size={16} strokeWidth={1.5} />
            Plan de trabajo
          </Link>
          <Link to="/empresa" className={clasesBoton('secundario')}>
            <Building2 aria-hidden size={16} strokeWidth={1.5} />
            Perfil de la empresa
          </Link>
        </div>
      </SiRol>
      <SiRol roles={[ROLES.adminEmpresa]}>
        <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Link to="/personas" className={clasesBoton('secundario')}>
            <Contact aria-hidden size={16} strokeWidth={1.5} />
            Ver las personas
          </Link>
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
