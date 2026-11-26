import { textoOpcional, z } from '@/shared/validacion'

/** Espejo de RevisarPeligroSugeridoValidator (backend) al confirmar: la actividad es opcional, hasta 200 caracteres. */
export const confirmarPeligroEsquema = z.object({ actividad: textoOpcional(200) })
