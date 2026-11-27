import { KeyRound, LogOut } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import type { UsuarioSesion } from '@/shared/api'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { cerrarSesion, etiquetaRol } from '@/shared/sesion'

/**
 * Encabezado del área privada: en qué empresa y con qué rol se está trabajando (HU-019: una sesión
 * por empresa), el enlace para cambiar la contraseña (HU-022) y el botón para cerrar sesión. El menú de cada módulo se agrega con su HU.
 * `acciones` va antes de esos botones: lo pone app/ con piezas de las features (la campana de notificaciones, HU-117),
 * porque shared/ no importa features.
 */
export function EncabezadoSesion({ usuario, acciones }: { usuario: UsuarioSesion; acciones?: ReactNode }) {
  const [cerrando, setCerrando] = useState(false)
  const lugar = usuario.tipo === 'plataforma' ? 'Plataforma Norvia' : (usuario.empresa?.razonSocial ?? 'Tu empresa')

  return (
    <header className="mb-8 flex flex-wrap items-center justify-between gap-4 border-b border-border-default pb-4">
      <div className="grid overflow-hidden">
        <span className="text-xs font-medium text-text-secondary">Norvia</span>
        <span className="truncate text-md font-semibold" title={lugar}>
          {lugar}
        </span>
        <span className="text-xs text-text-secondary">{etiquetaRol(usuario.rol)}</span>
      </div>
      <div className="flex flex-wrap items-center gap-2">
        {acciones}
        <Link to="/cuenta/contrasena" className={clasesBoton('secundario')}>
          <KeyRound aria-hidden size={16} strokeWidth={1.5} />
          Cambiar contraseña
        </Link>
        <Boton
          variante="secundario"
          cargando={cerrando}
          textoCargando="Cerrando sesión…"
          onClick={async () => {
            setCerrando(true)
            await cerrarSesion()
          }}
        >
          <LogOut aria-hidden size={16} strokeWidth={1.5} />
          Cerrar sesión
        </Boton>
      </div>
    </header>
  )
}
