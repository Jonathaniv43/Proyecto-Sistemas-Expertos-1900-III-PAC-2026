import { ClipboardList, House, ListChecks, ListTodo, ShieldAlert, Siren, type LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import type { UsuarioSesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Insignia } from '@/shared/components/ui/Insignia'
import { clasesBoton } from '@/shared/components/ui/clases'
import { EtiquetaRiesgo } from '@/shared/riesgo/EtiquetaRiesgo'
import { etiquetaRol } from '@/shared/sesion'

/** Dónde se entró, para el aviso de sesión iniciada. */
const lugarDe = (usuario: UsuarioSesion) =>
  usuario.tipo === 'plataforma' ? 'la administración de Norvia' : (usuario.empresa?.razonSocial ?? 'tu empresa')

/**
 * Portada de la landing (design-system.md § Landing): el título en Portada (40/48; Título 1 en celular), la acción
 * principal y una vista de la app real. Con sesión iniciada, en lugar de registrar, dice dónde se entró y ofrece ir al
 * inicio.
 */
export function Portada({ usuario }: { usuario: UsuarioSesion | null }) {
  return (
    <section id="inicio" className="border-b border-border-default bg-bg-surface">
      <div className="landing-portada margen-lateral mx-auto w-full max-w-layout">
        <div className="grid content-center gap-6">
          <h1 className="text-xl font-semibold tracking-tight text-balance lg:text-3xl">
            Conoce los riesgos de tu empresa, contrólalos y demuestra que cumples
          </h1>
          <p className="max-w-layout-narrow text-md text-text-secondary">
            Norvia guía paso a paso a las empresas hondureñas en la seguridad y salud en el trabajo.
          </p>
          {usuario ? (
            <>
              <Aviso tipo="info">
                Ya entraste a {lugarDe(usuario)} como {etiquetaRol(usuario.rol)}.
              </Aviso>
              <div className="flex flex-col gap-3 sm:flex-row">
                <Link to="/inicio" className={clasesBoton('primario')}>
                  Ir a mi inicio
                </Link>
              </div>
              <p className="text-text-secondary">Para entrar a otra empresa, cierra la sesión desde el menú de tu cuenta.</p>
            </>
          ) : (
            <div className="flex flex-col gap-3 sm:flex-row">
              <Link to="/registro" className={clasesBoton('primario')}>
                Registrar mi empresa
              </Link>
              <a href="#como-funciona" className={clasesBoton('secundario')}>
                Ver cómo funciona
              </a>
            </div>
          )}
        </div>
        <VistaApp />
      </div>
    </section>
  )
}

/**
 * La app como se ve: el menú y el inicio con lo pendiente (design-system.md § Inicio). Es una muestra, no lee datos;
 * el color de riesgo queda solo en su etiqueta, como en la app. Es decorativa: el texto la resume.
 */
function VistaApp() {
  return (
    <div role="img" aria-label="Ejemplo de Norvia: el inicio con lo pendiente de una empresa" className="landing-app">
      <div aria-hidden className="landing-app-menu">
        <div className="grid border-b border-border-default px-2 pb-3 text-xs">
          <span className="text-text-secondary">Norvia</span>
          <span className="font-semibold">Constructora Demo S.A.</span>
          <span className="text-text-secondary">Responsable SST</span>
        </div>
        <ItemMenu Icono={House} actual>
          Inicio
        </ItemMenu>
        <ItemMenu Icono={ClipboardList}>Encuestas</ItemMenu>
        <ItemMenu Icono={ShieldAlert}>Matriz de riesgos</ItemMenu>
        <ItemMenu Icono={ListChecks}>Plan de trabajo</ItemMenu>
        <ItemMenu Icono={Siren}>Eventos</ItemMenu>
      </div>
      <div aria-hidden className="grid content-start gap-3 p-4">
        <div className="grid">
          <span className="text-md font-semibold">Inicio</span>
          <span className="text-xs text-text-secondary">Lo que tienes pendiente en Constructora Demo S.A.</span>
        </div>
        <ul className="lista-pendientes text-xs">
          <Fila Icono={ListChecks} nombre="Plan de trabajo" detalle="1 tarea vencida · 1 sin responsable">
            <Insignia tipo="error">1 vencida</Insignia>
          </Fila>
          <Fila Icono={ShieldAlert} nombre="Matriz de riesgos" detalle="1 riesgo crítico sin control">
            <Insignia tipo="advertencia">Sin control</Insignia>
          </Fila>
          <Fila Icono={Siren} nombre="Eventos" detalle="2 eventos abiertos" />
          <Fila Icono={ListTodo} nombre="Charla de cinco minutos" detalle="Carla Colaboradora · 16 dic">
            <EtiquetaRiesgo nivel={{ nivel: 20, clasificacion: 'critico' }} />
          </Fila>
        </ul>
      </div>
    </div>
  )
}

function ItemMenu({ Icono, actual = false, children }: { Icono: LucideIcon; actual?: boolean; children: ReactNode }) {
  return (
    <span className={`flex items-center gap-2 rounded-sm px-2 py-1 text-xs font-medium ${actual ? 'bg-bg-selected text-text-selected' : 'text-text-secondary'}`}>
      <Icono aria-hidden size={16} strokeWidth={1.5} />
      {children}
    </span>
  )
}

function Fila({ Icono, nombre, detalle, children }: { Icono: LucideIcon; nombre: string; detalle: string; children?: ReactNode }) {
  return (
    <li className="flex items-center gap-2 px-3 py-2">
      <Icono aria-hidden size={16} strokeWidth={1.5} className="shrink-0 text-text-secondary" />
      <span className="grid min-w-0 flex-1">
        <span className="font-medium">{nombre}</span>
        <span className="text-text-secondary">{detalle}</span>
      </span>
      {children}
    </li>
  )
}
