import { z } from '@/shared/validacion'

export const MAX_DESCRIPCION_CONTROL = 1000

/** Espejo de CrearControlValidator (backend): tipo de la jerarquía (RN-21) y descripción de hasta 1000 caracteres. */
export const controlEsquema = z.object({
  tipo: z.enum(['eliminacion', 'sustitucion', 'ingenieria', 'administrativo', 'epp'], 'Elige el tipo de control.'),
  descripcion: z.string().trim().min(1, 'Describe el control.').max(MAX_DESCRIPCION_CONTROL, `Debe tener como máximo ${MAX_DESCRIPCION_CONTROL} caracteres.`),
})
