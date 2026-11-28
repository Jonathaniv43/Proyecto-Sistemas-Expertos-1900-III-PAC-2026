import { ChevronDown, ChevronUp, CircleUserRound, KeyRound, LogOut } from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import type { UsuarioSesion } from '@/shared/api'
import { cerrarSesion, etiquetaRol } from '@/shared/sesion'

interface MenuCuentaProps {
  usuario: UsuarioSesion
  /** Al elegir una opción que navega (el panel de celular se cierra). */
  onNavegar?: () => void
}

/**
 * Menú de cuenta al pie del menú (design-system.md § Menú de cuenta): el correo y, al abrirlo, el rol, cambiar o
 * agregar la contraseña (HU-022) y cerrar sesión. Se cierra con Esc (el foco vuelve al botón), al tocar fuera o al
 * elegir una opción.
 */
export function MenuCuenta({ usuario, onNavegar }: MenuCuentaProps) {
  const [abierto, setAbierto] = useState(false)
  const [cerrando, setCerrando] = useState(false)
  const boton = useRef<HTMLButtonElement>(null)
  const caja = useRef<HTMLDivElement>(null)
  const id = useId()

  useEffect(() => {
    if (!abierto) return
    caja.current?.querySelector<HTMLElement>('a, button')?.focus()
    const alTocar = (e: PointerEvent) => {
      const objetivo = e.target as Node
      if (!caja.current?.contains(objetivo) && !boton.current?.contains(objetivo)) setAbierto(false)
    }
    // En captura y con preventDefault: dentro del panel de celular, Esc cierra solo este menú y no el panel.
    const alTeclear = (e: KeyboardEvent) => {
      if (e.key !== 'Escape') return
      e.preventDefault()
      e.stopPropagation()
      setAbierto(false)
      boton.current?.focus()
    }
    document.addEventListener('pointerdown', alTocar)
    document.addEventListener('keydown', alTeclear, true)
    return () => {
      document.removeEventListener('pointerdown', alTocar)
      document.removeEventListener('keydown', alTeclear, true)
    }
  }, [abierto])

  const correo = usuario.email ?? 'Mi cuenta'
  const Chevron = abierto ? ChevronDown : ChevronUp

  return (
    <div className="menu-cuenta">
      {abierto && (
        <div ref={caja} id={id} className="menu-cuenta-caja">
          <p className="menu-cuenta-rol">{etiquetaRol(usuario.rol)}</p>
          <Link
            to="/cuenta/contrasena"
            className="item-menu"
            onClick={() => {
              setAbierto(false)
              onNavegar?.()
            }}
          >
            <KeyRound aria-hidden size={20} strokeWidth={1.5} />
            {usuario.tienePassword ? 'Cambiar contraseña' : 'Agregar contraseña'}
          </Link>
          <button
            type="button"
            className="item-menu"
            disabled={cerrando}
            aria-busy={cerrando}
            onClick={async () => {
              setCerrando(true)
              await cerrarSesion()
            }}
          >
            <LogOut aria-hidden size={20} strokeWidth={1.5} />
            {cerrando ? 'Cerrando sesión…' : 'Cerrar sesión'}
          </button>
        </div>
      )}
      <button
        ref={boton}
        type="button"
        className="item-menu"
        aria-expanded={abierto}
        aria-controls={abierto ? id : undefined}
        aria-label={`Tu cuenta: ${correo}`}
        onClick={() => setAbierto((a) => !a)}
      >
        <CircleUserRound aria-hidden size={20} strokeWidth={1.5} />
        <span className="min-w-0 flex-1 truncate text-left" title={correo}>
          {correo}
        </span>
        <Chevron aria-hidden size={16} strokeWidth={1.5} />
      </button>
    </div>
  )
}
