import { rtn, soloDigitos, textoOpcional, textoRequerido, z } from '@/shared/validacion'

/**
 * Espejo de RegistrarEmpresaMicrosoftValidator (backend, HU-119). No lleva correo (sale de Microsoft, RN-84)
 * ni contraseña. El documento es opcional y se completa después en el perfil, pero tipo y número van juntos.
 */
const TIPOS_DOCUMENTO = ['', 'dni', 'pasaporte'] as const

export const registroEmpresaMicrosoftEsquema = z
  .object({
    razonSocial: textoRequerido(200),
    rtn: rtn(),
    rubroCodigo: z.string().min(1, 'Elige el rubro de la empresa.').max(30),
    primerNombre: textoRequerido(80),
    segundoNombre: textoOpcional(80),
    primerApellido: textoRequerido(80),
    segundoApellido: textoOpcional(80),
    tipoDocumento: z.enum(TIPOS_DOCUMENTO),
    numeroDocumento: z.string().trim().max(50, 'Debe tener como máximo 50 caracteres.'),
  })
  .check((ctx) => {
    const { tipoDocumento } = ctx.value
    const numero = tipoDocumento === 'dni' ? soloDigitos(ctx.value.numeroDocumento) : ctx.value.numeroDocumento
    const sin = (campo: 'tipoDocumento' | 'numeroDocumento', message: string) =>
      ctx.issues.push({ code: 'custom', input: numero, path: [campo], message })

    if (!tipoDocumento && numero) sin('tipoDocumento', 'Elige el tipo de documento.')
    if (tipoDocumento && !numero) sin('numeroDocumento', 'Escribe el número del documento o quita el tipo.')
    if (tipoDocumento === 'dni' && numero && !/^\d{13}$/.test(numero))
      sin('numeroDocumento', 'El DNI tiene 13 dígitos. Escríbelo sin letras.')
    if (tipoDocumento === 'pasaporte' && numero && !/^[A-Za-z0-9-]+$/.test(numero))
      sin('numeroDocumento', 'El pasaporte solo lleva letras, números y guiones.')
  })
  .transform(({ tipoDocumento, numeroDocumento, ...datos }) => ({
    ...datos,
    tipoDocumento: tipoDocumento || undefined,
    numeroDocumento: !tipoDocumento ? undefined : tipoDocumento === 'dni' ? soloDigitos(numeroDocumento) : numeroDocumento,
  }))

export type RegistroEmpresaMicrosoftFormulario = z.input<typeof registroEmpresaMicrosoftEsquema>
