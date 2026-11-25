import { contrasenaIngresada, z } from '@/shared/validacion'

/** Espejo de VincularMicrosoftValidator: solo se compara con la contraseña guardada (RN-83). */
export const vincularMicrosoftEsquema = z.object({
  password: contrasenaIngresada(),
})
