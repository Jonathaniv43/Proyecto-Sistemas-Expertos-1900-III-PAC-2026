import { AlertCircle, AlertTriangle, CheckCircle2, Info } from 'lucide-react'
import type { AvisoProps, TipoAviso } from '../types/errors.types'

const estilos: Record<
  TipoAviso,
  { caja: string; icono: string; Icon: typeof Info }
> = {
  info: {
    caja: 'bg-info-bg border-info-border text-info-text',
    icono: 'text-info-icon',
    Icon: Info,
  },
  success: {
    caja: 'bg-success-bg border-success-border text-success-text',
    icono: 'text-success-icon',
    Icon: CheckCircle2,
  },
  warning: {
    caja: 'bg-warning-bg border-warning-border text-warning-text',
    icono: 'text-warning-icon',
    Icon: AlertTriangle,
  },
  error: {
    caja: 'bg-error-bg border-error-border text-error-text',
    icono: 'text-error-icon',
    Icon: AlertCircle,
  },
}

/**
 * Aviso inline del design system (info / éxito / advertencia / error).
 * Error al cargar pantalla → EstadoErrorCarga. Resultado puntual de acción → Toast.
 */
export function Aviso({ tipo = 'info', titulo, children, className = '' }: AvisoProps) {
  const { caja, icono, Icon } = estilos[tipo]

  return (
    <div
      role="status"
      className={`flex gap-3 items-start rounded-md border px-4 py-3 ${caja} ${className}`}
    >
      <Icon className={`size-4 shrink-0 mt-0.5 ${icono}`} aria-hidden strokeWidth={1.5} />
      <div className="min-w-0 text-sm leading-5">
        {titulo ? <p className="font-medium mb-0.5">{titulo}</p> : null}
        <div>{children}</div>
      </div>
    </div>
  )
}
