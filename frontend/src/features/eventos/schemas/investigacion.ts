import { textoAIso, textoRequerido, z } from '@/shared/validacion'

/**
 * Espejo de RegistrarInvestigacionValidator (backend) y de su regla de fecha: causas inmediatas y de fondo
 * (obligatorias, hasta 4000), conclusiones opcionales, quién investigó y la fecha, que no puede ser futura ni anterior
 * al evento (`desde` y `hasta` en aaaa-mm-dd).
 */
export const investigacionEsquema = (desde: string, hasta: string) =>
  z.object({
    causasInmediatas: textoRequerido(4000),
    causasBasicas: textoRequerido(4000),
    conclusiones: z.string().trim().max(4000, 'Debe tener como máximo 4000 caracteres.'),
    investigadaPorId: z.string().min(1, 'Elige quién investigó.'),
    fechaInvestigacion: z
      .string()
      .trim()
      .min(1, 'Indica la fecha de la investigación.')
      .transform((texto, ctx) => {
        const iso = textoAIso(texto)
        if (!iso) {
          ctx.addIssue({ code: 'custom', message: 'Escribe la fecha como dd/mm/aaaa.' })
          return z.NEVER
        }
        if (iso > hasta || iso < desde) {
          ctx.addIssue({ code: 'custom', message: 'No puede ser futura ni anterior al evento.' })
          return z.NEVER
        }
        return iso
      }),
  })
