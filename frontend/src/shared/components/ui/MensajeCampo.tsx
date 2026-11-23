import { CircleAlert } from 'lucide-react'
import type { ReactNode } from 'react'

interface MensajeCampoProps {
  id: string
  error?: string
  ayuda?: ReactNode
}

/** Error (ícono + texto en rojo) o, si no hay error, la ayuda del campo. Una sola línea bajo el control. */
export function MensajeCampo({ id, error, ayuda }: MensajeCampoProps) {
  if (error) {
    return (
      <p id={id} className="flex items-start gap-1 text-xs text-error-text">
        <span aria-hidden className="flex h-lh shrink-0 items-center text-error-icon">
          <CircleAlert size={14} strokeWidth={1.5} />
        </span>
        {error}
      </p>
    )
  }
  if (!ayuda) return null
  return (
    <p id={id} className="text-xs text-text-secondary">
      {ayuda}
    </p>
  )
}

export function Etiqueta({ htmlFor, children, required }: { htmlFor?: string; children: ReactNode; required?: boolean }) {
  return (
    <label htmlFor={htmlFor} className="text-sm font-medium text-text-primary">
      {children}
      {required && (
        <span aria-hidden className="text-error-text">
          {' *'}
        </span>
      )}
    </label>
  )
}
