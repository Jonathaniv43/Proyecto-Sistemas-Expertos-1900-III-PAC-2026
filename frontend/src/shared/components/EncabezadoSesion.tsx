import { LogOut } from 'lucide-react'
import { useState } from 'react'
import type { UsuarioSesion } from '@/shared/api'
import { Boton } from '@/shared/components/ui/Boton'
import { cerrarSesion, etiquetaRol } from '@/shared/sesion'

/**
 * Encabezado del área privada: en qué empresa y con qué rol se está trabajando (HU-019: una sesión
 * por empresa) y el botón para cerrar sesión. El menú de cada módulo se agrega con su HU.
 */
export function EncabezadoSesion({ usuario }: { usuario: UsuarioSesion }) {
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
    </header>
  )
}
