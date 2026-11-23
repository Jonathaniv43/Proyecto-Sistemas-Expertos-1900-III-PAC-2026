/** Contrato de POST /api/vinculos (HU-020). */
export interface CrearVinculoRequest {
  email: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
  rolCodigo: 'admin_empresa' | 'responsable_sst' | 'colaborador' | 'encuestado'
  /** Solo se usa si el correo no tiene cuenta en Norvia; si ya la tiene, el backend la ignora. */
  passwordTemporal: string
}

export interface CrearVinculoResponse {
  id: string
  personaId: string
  email: string
  nombreCompleto: string
  rolCodigo: string
  estado: 'activo' | 'suspendido' | 'baja'
  creadoEn: string
  /** true: se creó la cuenta con la contraseña temporal; false: se reutilizó una cuenta existente. */
  cuentaNueva: boolean
}
