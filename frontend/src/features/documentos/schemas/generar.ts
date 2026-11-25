import { z } from '@/shared/validacion'

/** Espejo de GenerarDocumentoValidator (backend): la plantilla es obligatoria y el título, opcional, de hasta 200. */
export const generarDocumentoEsquema = z.object({
  plantillaDocumentoId: z.string().min(1, 'Elige una plantilla.'),
  titulo: z.string().trim().max(200, 'Debe tener como máximo 200 caracteres.'),
})
