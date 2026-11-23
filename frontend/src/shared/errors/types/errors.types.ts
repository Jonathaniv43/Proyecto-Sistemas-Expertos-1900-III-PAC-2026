import type { ReactNode } from 'react'

/** Contratos de error de Norvia (RFC 7807 / ASP.NET ProblemDetails). */

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  instance?: string
  errors?: Record<string, string[]>
}

/** Resultado del parser para UI y formularios. */
export interface ErrorBackendParseado {
  mensaje: string
  erroresPorCampo?: Record<string, string[]>
  status?: number
}

/** Variantes de aviso del design system. */
export type TipoAviso = 'info' | 'success' | 'warning' | 'error'

export interface AvisoProps {
  tipo?: TipoAviso
  titulo?: string
  children: ReactNode
  className?: string
}

export interface MensajeErrorCampoProps {
  mensaje: string
  className?: string
}

export interface EstadoErrorCargaProps {
  titulo?: string
  detalle?: string
  onReintentar?: () => void
  className?: string
}

export interface AlertaErrorServidorProps {
  mensaje: string | null | undefined
  erroresPorCampo?: Record<string, string[]>
  titulo?: string
  onCerrar?: () => void
  className?: string
}
