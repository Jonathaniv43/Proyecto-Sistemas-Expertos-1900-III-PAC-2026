import { CircleAlert, CircleCheck, Info, TriangleAlert, type LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'

type Tipo = 'info' | 'exito' | 'advertencia' | 'error'

const ESTILOS: Record<Tipo, { caja: string; icono: string; Icono: LucideIcon }> = {
  info: { caja: 'border-info-border bg-info-bg text-info-text', icono: 'text-info-icon', Icono: Info },
  exito: { caja: 'border-success-border bg-success-bg text-success-text', icono: 'text-success-icon', Icono: CircleCheck },
  advertencia: { caja: 'border-warning-border bg-warning-bg text-warning-text', icono: 'text-warning-icon', Icono: TriangleAlert },
  error: { caja: 'border-error-border bg-error-bg text-error-text', icono: 'text-error-icon', Icono: CircleAlert },
}

interface AvisoProps {
  tipo: Tipo
  titulo?: string
  children: ReactNode
}

/** Aviso en línea (design-system.md § Avisos). No usa el color de marca ni se personaliza. */
export function Aviso({ tipo, titulo, children }: AvisoProps) {
  const { caja, icono, Icono } = ESTILOS[tipo]
  return (
    <div
      role={tipo === 'error' ? 'alert' : 'status'}
      className={`flex items-start gap-3 rounded-md border px-4 py-3 ${caja}`}
    >
      {/* h-lh: el contenedor mide una línea de texto, así el ícono queda centrado con la primera línea. */}
      <span aria-hidden className={`flex h-lh shrink-0 items-center ${icono}`}>
                <Icono size={16} strokeWidth={1.5} />
      </span>
      <div>
        {titulo && <p className="font-medium">{titulo}</p>}
        <div>{children}</div>
      </div>
    </div>
  )
}
