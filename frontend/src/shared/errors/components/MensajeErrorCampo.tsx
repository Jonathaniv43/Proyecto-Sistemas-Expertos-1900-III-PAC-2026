import { AlertCircle } from 'lucide-react'
import type { MensajeErrorCampoProps } from '../types/errors.types'

/**
 * Mensaje bajo un input / grupo (design system: ícono + --color-error-text).
 */
export function MensajeErrorCampo({ mensaje, className = '' }: MensajeErrorCampoProps) {
  if (!mensaje) return null

  return (
    <p
      className={`mt-2 flex items-center gap-2 text-xs leading-4 text-error-text ${className}`}
      role="alert"
    >
      <AlertCircle className="size-4 shrink-0 text-error-icon" aria-hidden strokeWidth={1.5} />
      <span>{mensaje}</span>
    </p>
  )
}
