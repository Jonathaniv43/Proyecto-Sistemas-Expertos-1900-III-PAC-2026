/**
 * Etiqueta de riesgo (design-system.md § Etiqueta de riesgo): chip neutro; solo el cuadrado lleva el
 * color del nivel y el texto dice el nivel y el número. Sin evaluación, "Sin evaluar" (HU-037 evalúa).
 */
export function EtiquetaRiesgo() {
  return <span className="etiqueta-riesgo text-text-secondary">Sin evaluar</span>
}
