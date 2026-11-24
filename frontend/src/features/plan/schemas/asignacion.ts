import { fechaLimite, z } from '@/shared/validacion'

/**
 * Espejo de AsignarTareaValidator (backend): responsable y fecha límite obligatorios (RN-22); la fecha no puede ser
 * anterior a hoy, salvo que se conserve la que tenía (`fechaActual`).
 */
export const asignacionEsquema = (fechaActual: string | null) =>
  z.object({
    responsableId: z.string().min(1, 'Elige a la persona responsable.'),
    fechaLimite: fechaLimite(fechaActual),
  })
