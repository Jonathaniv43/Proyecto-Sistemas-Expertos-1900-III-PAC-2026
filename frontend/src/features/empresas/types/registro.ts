/** Contratos de POST /api/empresas y GET /api/empresas/rubros-registro (HU-017). */

export interface RubroRegistro {
  codigo: string
  nombre: string
}

export interface RegistrarEmpresaRequest {
  razonSocial: string
  rtn: string
  rubroCodigo: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
  tipoDocumento: 'dni' | 'pasaporte'
  numeroDocumento: string
  email: string
}

export interface RegistrarEmpresaResponse {
  /** Identificador de empresa cifrado (para el login). */
  idEmpresa: string
  razonSocial: string
  rtn: string
  vinculoId: string
  email: string
  /** false si no se pudo enviar el correo con la contraseña temporal. */
  correoEnviado: boolean
}

/** POST /api/empresas/microsoft (HU-119): sin correo ni contraseña; el correo sale del token de identidad. */
export interface RegistrarEmpresaMicrosoftRequest {
  tokenIdentidad: string
  razonSocial: string
  rtn: string
  rubroCodigo: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
  tipoDocumento?: 'dni' | 'pasaporte'
  numeroDocumento?: string
}
