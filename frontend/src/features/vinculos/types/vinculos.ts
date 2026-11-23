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

/** POST /api/vinculos/encuestados (HU-115). Sin contraseña: el backend la genera si la cuenta es nueva. */
export interface CrearEncuestadoRequest {
  email: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
}

export interface CrearEncuestadoResponse extends Omit<CrearVinculoResponse, 'rolCodigo'> {
  rolCodigo: 'encuestado'
  /** Solo en cuentas nuevas: la contraseña generada, visible una única vez. */
  passwordTemporal: string | null
}

/** POST /api/vinculos/encuestados/lote: hasta 500 filas; cada una se procesa aunque otras fallen. */
export type FilaEncuestado = CrearEncuestadoRequest

export interface EncuestadoCreado {
  /** Posición en la lista enviada, desde 1. */
  fila: number
  vinculoId: string
  email: string
  nombreCompleto: string
  cuentaNueva: boolean
  passwordTemporal: string | null
}

export interface ErrorFila {
  fila: number
  email: string
  codigo: string
  mensaje: string
}

export interface CrearEncuestadosLoteResponse {
  creados: number
  cuentas: EncuestadoCreado[]
  errores: ErrorFila[]
}
