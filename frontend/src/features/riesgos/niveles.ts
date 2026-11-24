import type { ClasificacionRiesgo } from './types/riesgos'

/**
 * Escalas de la evaluación (RN-15, RN-80): cada valor del 1 al 5 con su explicación, que se muestra al
 * evaluar. Propuesta por validar con una persona de SST, como la clasificación de RN-16.
 */
export const PROBABILIDADES = [
  { valor: 1, nombre: 'Rara', explicacion: 'Solo en circunstancias excepcionales; no ha pasado en la empresa.' },
  { valor: 2, nombre: 'Improbable', explicacion: 'Podría pasar, pero no se espera; ha pasado alguna vez en el sector.' },
  { valor: 3, nombre: 'Posible', explicacion: 'Podría pasar alguna vez; ya ha pasado en la empresa.' },
  { valor: 4, nombre: 'Probable', explicacion: 'Es de esperar que pase; ha pasado varias veces en la empresa.' },
  { valor: 5, nombre: 'Casi segura', explicacion: 'Pasa o se espera que pase con frecuencia, cada mes o más.' },
] as const

export const SEVERIDADES = [
  { valor: 1, nombre: 'Insignificante', explicacion: 'Sin lesión, o primeros auxilios sin perder tiempo de trabajo.' },
  { valor: 2, nombre: 'Menor', explicacion: 'Lesión leve con atención médica, sin incapacidad o de un día.' },
  { valor: 3, nombre: 'Moderada', explicacion: 'Lesión con incapacidad temporal de más de un día.' },
  { valor: 4, nombre: 'Mayor', explicacion: 'Lesión grave o enfermedad con incapacidad permanente parcial.' },
  { valor: 5, nombre: 'Catastrófica', explicacion: 'Muerte, incapacidad permanente total o varias personas afectadas.' },
] as const

export const ETIQUETAS_CLASIFICACION_RIESGO: Record<ClasificacionRiesgo, string> = {
  bajo: 'Bajo',
  medio: 'Medio',
  alto: 'Alto',
  critico: 'Crítico',
}

/** RN-16, igual que el backend: solo para la vista previa mientras se elige; el valor guardado lo calcula la API. */
export const clasificar = (nivel: number): ClasificacionRiesgo =>
  nivel <= 4 ? 'bajo' : nivel <= 9 ? 'medio' : nivel <= 14 ? 'alto' : 'critico'

/** Clases del color de cada nivel (design-system.md § Semánticas: riesgo): fondo y texto encima. */
export const CLASES_NIVEL: Record<ClasificacionRiesgo, string> = {
  bajo: 'bg-riesgo-bajo text-riesgo-bajo-texto',
  medio: 'bg-riesgo-medio text-riesgo-medio-texto',
  alto: 'bg-riesgo-alto text-riesgo-alto-texto',
  critico: 'bg-riesgo-critico text-riesgo-critico-texto',
}
