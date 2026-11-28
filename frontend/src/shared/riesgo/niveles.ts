/**
 * Clasificación del nivel de riesgo (RN-16): 1–4 bajo, 5–9 medio, 10–14 alto, 15–25 crítico. Compartida porque
 * la matriz (riesgos) y el plan de trabajo muestran el nivel con la misma etiqueta.
 */
export type ClasificacionRiesgo = 'bajo' | 'medio' | 'alto' | 'critico'

export const ETIQUETAS_CLASIFICACION_RIESGO: Record<ClasificacionRiesgo, string> = {
  bajo: 'Bajo',
  medio: 'Medio',
  alto: 'Alto',
  critico: 'Crítico',
}

/** Clases del color de cada nivel (design-system.md § Semánticas: riesgo): fondo y texto encima. */
export const CLASES_NIVEL: Record<ClasificacionRiesgo, string> = {
  bajo: 'bg-riesgo-bajo text-riesgo-bajo-texto',
  medio: 'bg-riesgo-medio text-riesgo-medio-texto',
  alto: 'bg-riesgo-alto text-riesgo-alto-texto',
  critico: 'bg-riesgo-critico text-riesgo-critico-texto',
}

/** Clasificación de un nivel (probabilidad × severidad) según RN-16. */
export const clasificacionDe = (nivel: number): ClasificacionRiesgo =>
  nivel >= 15 ? 'critico' : nivel >= 10 ? 'alto' : nivel >= 5 ? 'medio' : 'bajo'
