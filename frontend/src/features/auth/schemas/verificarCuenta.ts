import { contrasenaNueva, correo, z } from '@/shared/validacion'

/** Espejo de VerificarCuentaValidator (backend). El código es el mismo mecanismo que el de Microsoft (RN-86). */
export const verificarCuentaEsquema = z
  .object({
    email: correo(),
    // Se aceptan espacios al pegar el código (igual que VerificarCorreoMicrosoft).
    codigo: z
      .string()
      .trim()
      .min(1, 'Escribe el código de 6 dígitos que te enviamos por correo.')
      .transform((valor) => valor.replace(/\s/g, ''))
      .pipe(z.string().regex(/^\d{6}$/, 'El código tiene 6 dígitos.')),
    contrasenaNueva: contrasenaNueva(),
    contrasenaNuevaRepetida: z.string().min(1, 'Vuelve a escribir la contraseña nueva.'),
  })
  // .check corre aunque otros campos tengan errores: la comparación se ve mientras se escribe.
  .check((ctx) => {
    const { contrasenaNueva: nueva, contrasenaNuevaRepetida } = ctx.value
    if (contrasenaNuevaRepetida && contrasenaNuevaRepetida !== nueva) {
      ctx.issues.push({
        code: 'custom',
        input: contrasenaNuevaRepetida,
        path: ['contrasenaNuevaRepetida'],
        message: 'Las contraseñas no coinciden.',
      })
    }
  })
