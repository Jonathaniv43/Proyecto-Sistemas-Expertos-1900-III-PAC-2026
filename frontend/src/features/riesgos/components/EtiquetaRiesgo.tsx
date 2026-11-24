import { CLASES_NIVEL, ETIQUETAS_CLASIFICACION_RIESGO } from '../niveles'
import type { ClasificacionRiesgo } from '../types/riesgos'

/**
 * Etiqueta de riesgo (design-system.md § Etiqueta de riesgo): chip neutro; solo el cuadrado lleva el
 * color del nivel y el texto dice el nivel y el número ("Alto · 12"). Sin evaluación, "Sin evaluar".
 */
export function EtiquetaRiesgo({ nivel }: { nivel?: { nivel: number; clasificacion: ClasificacionRiesgo } | null }) {
  if (!nivel) return <span className="etiqueta-riesgo text-text-secondary">Sin evaluar</span>
  return (
    <span className="etiqueta-riesgo text-text-primary">
      <span aria-hidden className={`cuadro-riesgo ${CLASES_NIVEL[nivel.clasificacion]}`} />
      {ETIQUETAS_CLASIFICACION_RIESGO[nivel.clasificacion]} · {nivel.nivel}
    </span>
  )
}
