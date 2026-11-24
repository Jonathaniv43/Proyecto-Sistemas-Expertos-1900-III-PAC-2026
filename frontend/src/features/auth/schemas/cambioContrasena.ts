import { contrasenaNueva, correo, z } from '@/shared/validacion'

/** Espejo de SolicitarCambioContrasenaValidator (backend). */
export const solicitarCambioEsquema = z.object({ email: correo() })

/** Espejo de ConfirmarCambioContrasenaValidator (backend); la repetición solo se compara en el front. */
export const restablecerContrasenaEsquema = z
  .object({
    contrasenaNueva: contrasenaNueva(),
    contrasenaNuevaRepetida: z.string().min(1, 'Vuelve a escribir la contraseña nueva.'),
  })
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
