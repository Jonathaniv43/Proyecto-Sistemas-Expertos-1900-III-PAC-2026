import { Menu, X } from 'lucide-react'
import { useEffect, useRef, useState, type ReactNode } from 'react'
import type { UsuarioSesion } from '@/shared/api'
import { useEsEscritorio } from '@/shared/hooks/useEsEscritorio'
import { menuDe } from '@/shared/navegacion'
import { etiquetaRol } from '@/shared/sesion'
import { MenuCuenta } from './MenuCuenta'
import { MenuNavegacion } from './MenuNavegacion'

interface LayoutSesionProps {
  usuario: UsuarioSesion
  /** Junto a la empresa: la campana de notificaciones (HU-117). La pone app/, porque shared/ no importa features. */
  acciones?: ReactNode
  children: ReactNode
}

/** Dónde se está trabajando (HU-019: una sesión por empresa): la empresa o "Plataforma Norvia", y el rol. */
const lugarDe = (usuario: UsuarioSesion) =>
  usuario.tipo === 'plataforma' ? 'Plataforma Norvia' : (usuario.empresa?.razonSocial ?? 'Tu empresa')

/**
 * Layout de empresa y de plataforma (design-system.md § Layouts y navegación). Mismo marco para las dos sesiones;
 * cambian el lugar, el menú del rol y la campana (solo empresa).
 * - Escritorio (≥1024px): menú lateral fijo con la identidad arriba y la cuenta abajo.
 * - Celular y tablet: barra superior (menú, empresa y campana) y el mismo menú en un panel lateral.
 */
export function LayoutSesion({ usuario, acciones, children }: LayoutSesionProps) {
  const escritorio = useEsEscritorio()
  const [panelAbierto, setPanelAbierto] = useState(false)
  const grupos = menuDe(usuario.rol)
  const lugar = lugarDe(usuario)
  const rol = etiquetaRol(usuario.rol)

  const identidad = (extra: ReactNode) => (
    <div className="menu-identidad">
      <div className="grid min-w-0">
        <span className="text-xs text-text-secondary">Norvia</span>
        <span className="menu-identidad-lugar" title={lugar}>
          {lugar}
        </span>
        <span className="text-xs text-text-secondary">{rol}</span>
      </div>
      {extra}
    </div>
  )

  return (
    <div className="layout-sesion" data-menu={escritorio ? 'lateral' : 'panel'}>
      {escritorio ? (
        <aside className="menu-lateral">
          {identidad(acciones)}
          <MenuNavegacion grupos={grupos} />
          <MenuCuenta usuario={usuario} />
        </aside>
      ) : (
        <>
          <header className="barra-superior">
            <button
              type="button"
              className="boton-icono"
              aria-label="Abrir el menú"
              aria-expanded={panelAbierto}
              aria-controls="panel-menu"
              onClick={() => setPanelAbierto(true)}
            >
              <Menu aria-hidden size={20} strokeWidth={1.5} />
            </button>
            <div className="grid min-w-0 flex-1">
              <span className="truncate font-semibold" title={lugar}>
                {lugar}
              </span>
              <span className="text-xs text-text-secondary">{rol}</span>
            </div>
            {acciones}
          </header>
          <PanelMenu abierto={panelAbierto} onCerrar={() => setPanelAbierto(false)}>
            {identidad(
              <button type="button" className="boton-icono" aria-label="Cerrar el menú" onClick={() => setPanelAbierto(false)}>
                <X aria-hidden size={20} strokeWidth={1.5} />
              </button>,
            )}
            <MenuNavegacion grupos={grupos} onNavegar={() => setPanelAbierto(false)} />
            <MenuCuenta usuario={usuario} onNavegar={() => setPanelAbierto(false)} />
          </PanelMenu>
        </>
      )}
      <main className="pagina contenido-sesion">{children}</main>
    </div>
  )
}

/**
 * Panel del menú en celular: <dialog> modal, así el foco queda dentro, Esc lo cierra y al cerrarse el foco vuelve al
 * botón que lo abrió. También se cierra al tocar el fondo o al elegir un destino.
 */
function PanelMenu({ abierto, onCerrar, children }: { abierto: boolean; onCerrar: () => void; children: ReactNode }) {
  const dialogo = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const el = dialogo.current
    if (!el) return
    if (abierto && !el.open) el.showModal()
    else if (!abierto && el.open) el.close()
  }, [abierto])

  return (
    <dialog
      ref={dialogo}
      id="panel-menu"
      aria-label="Menú"
      className="panel-menu"
      onClose={onCerrar}
      // El fondo es el propio <dialog> fuera de su contenido.
      onClick={(e) => {
        if (e.target === e.currentTarget) onCerrar()
      }}
    >
      {children}
    </dialog>
  )
}
