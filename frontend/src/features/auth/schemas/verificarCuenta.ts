import { contrasenaNueva, correo, z } from '@/shared/validacion'

/** Espejo de VerificarCuentaValidator (backend). */
export const verificarCuentaEsquema = z
  .object({
    email: correo(),
    contrasenaTemporal: z.string().min(1, 'Ingresa la contraseña temporal que te enviamos por correo.'),
    contrasenaNueva: contrasenaNueva(),
    contrasenaNuevaRepetida: z.string().min(1, 'Vuelve a escribir la contraseña nueva.'),
  })
  // .check corre aunque otros campos tengan errores: las comparaciones se ven mientras se escribe.
  .check((ctx) => {
    const { contrasenaTemporal, contrasenaNueva: nueva, contrasenaNuevaRepetida } = ctx.value
    if (nueva && nueva === contrasenaTemporal) {
      ctx.issues.push({
        code: 'custom',
        input: nueva,
        path: ['contrasenaNueva'],
        message: 'La contraseña nueva debe ser distinta de la temporal.',
      })
    }
    if (contrasenaNuevaRepetida && contrasenaNuevaRepetida !== nueva) {
      ctx.issues.push({
        code: 'custom',
        input: contrasenaNuevaRepetida,
        path: ['contrasenaNuevaRepetida'],
        message: 'Las contraseñas no coinciden.',
      })
    }
  })
