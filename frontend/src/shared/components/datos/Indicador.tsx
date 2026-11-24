import type { ReactNode } from 'react'

/**
 * Indicador (design-system.md § Resultados de encuesta, § Estadísticas de eventos): etiqueta, valor grande y una
 * aclaración opcional. Va dentro de un <dl>.
 */
export function Indicador({ etiqueta, valor, detalle }: { etiqueta: string; valor: ReactNode; detalle?: string }) {
  return (
    <div className="grid content-start gap-1 rounded-lg border border-border-default bg-bg-surface p-4">
      <dt className="text-xs text-text-secondary">{etiqueta}</dt>
      <dd className="text-2xl font-semibold tabular-nums">{valor}</dd>
      {detalle && <dd className="text-xs text-text-secondary">{detalle}</dd>}
    </div>
  )
}
