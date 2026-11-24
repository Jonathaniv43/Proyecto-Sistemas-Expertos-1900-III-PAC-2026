import { contrasenaIngresada, contrasenaNueva, correo, rtn, z } from '@/shared/validacion'

/** Paso 1 del login: el RTN identifica la empresa (ResolverEmpresaPorRtnValidator). */
export const buscarEmpresaEsquema = z.object({
  rtn: rtn(),
})

/** Paso 2 del login: espejo de LoginEmpresaValidator; la empresa ya viene resuelta por el RTN del URL. */
export const loginEmpresaEsquema = z.object({
  email: correo(),
  password: contrasenaIngresada(),
})

/** Login del equipo de Norvia (LoginPlataformaValidator). */
export const loginPlataformaEsquema = z.object({
  email: correo(),
  password: contrasenaIngresada(),
})

/** Cambio obligatorio de la contraseña temporal: espejo de ReestablecerContrasenaValidator + confirmación. */
export const cambiarContrasenaEsquema = z
  .object({
    contrasenaActual: z.string().min(1, 'Ingresa la contraseña con la que entraste.'),
    contrasenaNueva: contrasenaNueva(),
    contrasenaNuevaRepetida: z.string().min(1, 'Vuelve a escribir la contraseña nueva.'),
  })
  .check((ctx) => {
    const { contrasenaActual, contrasenaNueva: nueva, contrasenaNuevaRepetida } = ctx.value
    if (nueva && nueva === contrasenaActual) {
      ctx.issues.push({
        code: 'custom',
        input: nueva,
        path: ['contrasenaNueva'],
        message: 'La contraseña nueva debe ser distinta de la actual.',
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
