import { correo, textoOpcional, textoRequerido, z } from '@/shared/validacion'

/** Espejo de CrearEncuestadoValidator (sin contraseña: la genera el backend). */
export const encuestadoEsquema = z.object({
  email: correo(),
  primerNombre: textoRequerido(80),
  segundoNombre: textoOpcional(80),
  primerApellido: textoRequerido(80),
  segundoApellido: textoOpcional(80),
})

/** Espejo de FilaEncuestadoValidator: una fila del lote, con mensajes cortos para la tabla. */
export const filaEncuestadoEsquema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Falta el correo.')
    .max(254, 'El correo es demasiado largo.')
    .pipe(z.email('El correo no es válido.')),
  primerNombre: z.string().trim().min(1, 'Falta el primer nombre.').max(80, 'El primer nombre es demasiado largo.'),
  segundoNombre: textoOpcional(80),
  primerApellido: z.string().trim().min(1, 'Falta el primer apellido.').max(80, 'El primer apellido es demasiado largo.'),
  segundoApellido: textoOpcional(80),
})

/** CrearEncuestadosLoteValidator.MaxFilas */
export const MAXIMO_FILAS_LOTE = 500
