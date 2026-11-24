import { fechaLimite, textoOpcional, z } from '@/shared/validacion'

/**
 * Tarea que nace asignada: una tarea manual del plan (HU-043) o una medida correctiva de un evento (HU-050).
 * Espejo de CrearTareaManualValidator y CrearMedidaCorrectivaValidator (backend): título (hasta 200), descripción
 * opcional (hasta 2000), prioridad, responsable y fecha límite no anterior a hoy.
 */
export const tareaAsignadaEsquema = z.object({
  titulo: z.string().trim().min(1, 'Escribe qué hay que hacer.').max(200, 'Debe tener como máximo 200 caracteres.'),
  descripcion: textoOpcional(2000),
  prioridad: z.enum(['1', '2', '3'], 'Elige la prioridad.').transform((p) => Number(p) as 1 | 2 | 3),
  responsableId: z.string().min(1, 'Elige a la persona responsable.'),
  fechaLimite: fechaLimite(),
  requiereEvidencia: z.boolean(),
})

/** Opciones de prioridad para `GrupoRadio`. */
export const OPCIONES_PRIORIDAD = [
  { valor: '1', texto: 'Alta', descripcion: 'Hay que atenderla primero.' },
  { valor: '2', texto: 'Media' },
  { valor: '3', texto: 'Baja', descripcion: 'Puede esperar a las demás.' },
]
