import { z } from '@/shared/validacion'

/** Espejo de CrearControlValidator (backend): tipo de la jerarquía (RN-21) y descripción de hasta 1000 caracteres. */
export const controlEsquema = z.object({
  tipo: z.enum(['eliminacion', 'sustitucion', 'ingenieria', 'administrativo', 'epp'], 'Elige el tipo de control.'),
  descripcion: z.string().trim().min(1, 'Describe el control.').max(1000, 'Debe tener como máximo 1000 caracteres.'),
})
