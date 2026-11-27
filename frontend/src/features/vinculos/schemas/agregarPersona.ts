import { ROLES_EMPRESA } from '@/shared/sesion'
import { correo, textoOpcional, textoRequerido, z } from '@/shared/validacion'

/** Espejo de CrearVinculoValidator (backend). Si la cuenta es nueva, el código de activación lo envía el backend. */
export const agregarPersonaEsquema = z.object({
  email: correo(),
  primerNombre: textoRequerido(80),
  segundoNombre: textoOpcional(80),
  primerApellido: textoRequerido(80),
  segundoApellido: textoOpcional(80),
  rolCodigo: z.enum(ROLES_EMPRESA, 'Elige el rol que tendrá en la empresa.'),
})
