import { correo, rtn, soloDigitos, textoOpcional, textoRequerido, z } from '@/shared/validacion'

/**
 * Espejo de RegistrarEmpresaValidator (backend). Los datos del perfil (HU-026) no se piden aquí.
 * RTN y DNI se aceptan con guiones o espacios y se envían solo con los dígitos.
 */
const TIPOS_DOCUMENTO = ['dni', 'pasaporte'] as const

export const registroEmpresaEsquema = z
  .object({
    razonSocial: textoRequerido(200),
    rtn: rtn(),
    rubroCodigo: z.string().min(1, 'Elige el rubro de la empresa.').max(30),
    primerNombre: textoRequerido(80),
    segundoNombre: textoOpcional(80),
    primerApellido: textoRequerido(80),
    segundoApellido: textoOpcional(80),
    tipoDocumento: z.enum(TIPOS_DOCUMENTO, 'Elige el tipo de documento.'),
    numeroDocumento: textoRequerido(50),
    email: correo(),
  })
  // .check corre aunque otros campos tengan errores, así el error del documento aparece en vivo.
  .check((ctx) => {
    const { tipoDocumento } = ctx.value
    const numeroDocumento = tipoDocumento === 'dni' ? soloDigitos(ctx.value.numeroDocumento) : ctx.value.numeroDocumento
    if (tipoDocumento === 'dni' && !/^\d{13}$/.test(numeroDocumento)) {
      ctx.issues.push({
        code: 'custom',
        input: numeroDocumento,
        path: ['numeroDocumento'],
        message: 'El DNI tiene 13 dígitos. Escríbelo sin letras.',
      })
    }
    if (tipoDocumento === 'pasaporte' && !/^[A-Za-z0-9-]+$/.test(numeroDocumento)) {
      ctx.issues.push({
        code: 'custom',
        input: numeroDocumento,
        path: ['numeroDocumento'],
        message: 'El pasaporte solo lleva letras, números y guiones.',
      })
    }
  })
  .transform((datos) =>
    datos.tipoDocumento === 'dni' ? { ...datos, numeroDocumento: soloDigitos(datos.numeroDocumento) } : datos,
  )

export type RegistroEmpresaFormulario = z.input<typeof registroEmpresaEsquema>
