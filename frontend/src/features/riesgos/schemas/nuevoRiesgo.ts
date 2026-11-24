import { textoRequerido, z } from '@/shared/validacion'

/**
 * Espejo de CrearRiesgoValidator (backend): un peligro del catálogo o uno descrito (hasta 300
 * caracteres), y la actividad relacionada (obligatoria, hasta 200).
 */
export const nuevoRiesgoEsquema = z
  .object({
    origen: z.enum(['catalogo', 'descrito'], 'Elige de dónde sale el peligro.'),
    peligroId: z.string(),
    descripcionPeligro: z.string().trim().max(300, 'Debe tener como máximo 300 caracteres.'),
    actividad: textoRequerido(200),
  })
  .check((ctx) => {
    const { origen, peligroId, descripcionPeligro } = ctx.value
    if (origen === 'catalogo' && !peligroId)
      ctx.issues.push({ code: 'custom', input: peligroId, path: ['peligroId'], message: 'Elige un peligro del catálogo.' })
    if (origen === 'descrito' && !descripcionPeligro.trim())
      ctx.issues.push({ code: 'custom', input: descripcionPeligro, path: ['descripcionPeligro'], message: 'Describe el peligro.' })
  })
