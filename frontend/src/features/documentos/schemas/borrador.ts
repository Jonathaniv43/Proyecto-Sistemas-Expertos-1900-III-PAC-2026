import { z } from '@/shared/validacion'

/** Espejo de EditarBorradorValidator (backend): el documento no queda vacío y tiene hasta 50 000 caracteres. */
export const LARGO_MAXIMO_DOCUMENTO = 50_000

export const borradorEsquema = z.object({
  contenido: z
    .string()
    .refine((texto) => texto.trim().length > 0, 'El documento no puede quedar vacío.')
    .refine((texto) => texto.length <= LARGO_MAXIMO_DOCUMENTO, `Debe tener como máximo ${LARGO_MAXIMO_DOCUMENTO} caracteres.`),
})
