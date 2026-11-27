import { contrasenaIngresada, correo, rtn, z } from '@/shared/validacion'

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
