import type { ReactNode } from 'react'

type TipoInsignia = 'neutro' | 'info' | 'exito' | 'advertencia' | 'error'

const ESTILOS: Record<TipoInsignia, string> = {
  neutro: 'border-border-default bg-bg-subtle text-text-secondary',
  info: 'border-info-border bg-info-bg text-info-text',
  exito: 'border-success-border bg-success-bg text-success-text',
  advertencia: 'border-warning-border bg-warning-bg text-warning-text',
  error: 'border-error-border bg-error-bg text-error-text',
}

/**
 * Insignia de estado (design-system.md § Badges de estado). Cada color significa siempre lo mismo:
 * neutro = sin empezar o fuera de uso, info = en curso, éxito = vigente, advertencia = atención,
 * error = algo salió mal. La marca no comunica estados.
 */
export function Insignia({ tipo, children }: { tipo: TipoInsignia; children: ReactNode }) {
  return <span className={`insignia ${ESTILOS[tipo]}`}>{children}</span>
}
