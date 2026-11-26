import { z } from '@/shared/validacion'

const VALORES = ['1', '2', '3', '4', '5'] as const

/** Espejo de EvaluarRiesgoValidator (backend): probabilidad y severidad del 1 al 5 (RN-15). */
export const evaluacionEsquema = z.object({
  probabilidad: z.enum(VALORES, 'Elige la probabilidad.').transform(Number),
  severidad: z.enum(VALORES, 'Elige la severidad.').transform(Number),
})
