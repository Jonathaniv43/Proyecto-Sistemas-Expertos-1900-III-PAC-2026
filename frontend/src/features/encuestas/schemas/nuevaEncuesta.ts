import { textoRequerido, z } from '@/shared/validacion'

/** Espejo de CrearEncuestaValidator: título (máx. 200) y al menos una categoría, sin repetir. */
export const nuevaEncuestaEsquema = z.object({
  titulo: textoRequerido(200),
  categoriaRiesgoIds: z
    .array(z.string())
    .min(1, 'Elige al menos una categoría de riesgo.')
    .refine((ids) => new Set(ids).size === ids.length, 'No repitas categorías.'),
})
