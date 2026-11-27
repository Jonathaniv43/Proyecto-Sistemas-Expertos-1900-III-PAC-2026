/** Contrato de POST /api/vinculos (HU-020). */
export interface CrearVinculoRequest {
  email: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
  rolCodigo: 'admin_empresa' | 'responsable_sst' | 'colaborador' | 'encuestado'
}

export interface CrearVinculoResponse {
  id: string
  personaId: string
  email: string
  nombreCompleto: string
  rolCodigo: string
  estado: 'activo' | 'suspendido' | 'baja'
  creadoEn: string
  /** true: se creó la cuenta (le llegó un código de 6 dígitos para activarla); false: se reutilizó una existente. */
  cuentaNueva: boolean
  /** Solo tiene sentido si `cuentaNueva` es true. */
  correoActivacionEnviado: boolean
}

/** POST /api/vinculos/encuestados (HU-115). Sin contraseña: si la cuenta es nueva, el backend envía el código. */
export interface CrearEncuestadoRequest {
  email: string
  primerNombre: string
  segundoNombre?: string
  primerApellido: string
  segundoApellido?: string
}

export interface CrearEncuestadoResponse extends Omit<CrearVinculoResponse, 'rolCodigo'> {
  rolCodigo: 'encuestado'
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
  /** Solo tiene sentido si `cuentaNueva` es true. */
  correoActivacionEnviado: boolean
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

/** Estado del acceso de una persona a la empresa (D22). */
export type EstadoVinculo = 'activo' | 'suspendido' | 'baja'

/** Motivo de un cambio de estado (RN-59). */
export type MotivoEstadoVinculo = 'decision_admin' | 'baja_laboral' | 'falta_pago_empresa' | 'otro'

/** GET /api/vinculos y PATCH /api/vinculos/{id} (HU-021). */
export interface Vinculo {
  id: string
  personaId: string
  email: string
  nombreCompleto: string
  rolCodigo: string
  estado: EstadoVinculo
  creadoEn: string
}

/** PATCH /api/vinculos/{id}: se envía solo lo que cambia. El motivo es obligatorio al suspender o dar de baja. */
export interface ActualizarVinculoRequest {
  rolCodigo?: string
  estado?: EstadoVinculo
  motivoCodigo?: MotivoEstadoVinculo
  motivoDetalle?: string
}
