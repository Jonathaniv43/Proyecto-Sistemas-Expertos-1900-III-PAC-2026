import { fechaLimite, textoOpcional, z } from '@/shared/validacion'

/**
 * Espejo de CrearTareaManualValidator (backend): título (hasta 200), descripción opcional (hasta 2000), prioridad,
 * responsable y fecha límite no anterior a hoy.
 */
export const tareaManualEsquema = z.object({
  titulo: z.string().trim().min(1, 'Escribe qué hay que hacer.').max(200, 'Debe tener como máximo 200 caracteres.'),
  descripcion: textoOpcional(2000),
  prioridad: z.enum(['1', '2', '3'], 'Elige la prioridad.').transform((p) => Number(p) as 1 | 2 | 3),
  responsableId: z.string().min(1, 'Elige a la persona responsable.'),
  fechaLimite: fechaLimite(),
  requiereEvidencia: z.boolean(),
})
