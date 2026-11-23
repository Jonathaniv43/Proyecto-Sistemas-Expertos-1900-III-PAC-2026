import { AlertCircle } from 'lucide-react'
import type { EstadoErrorCargaProps } from '../types/errors.types'

/**
 * Estado de pantalla: error al cargar (design system).
 * En el lugar del contenido, no en un toast.
 */
export function EstadoErrorCarga({
  titulo = 'No se pudo cargar la información',
  detalle = 'Revisa tu conexión e inténtalo de nuevo.',
  onReintentar,
  className = '',
}: EstadoErrorCargaProps) {
  return (
    <div
      className={`flex flex-col items-center justify-center text-center py-16 px-4 ${className}`}
      role="alert"
    >
      <AlertCircle
        className="size-8 text-error-icon mb-4"
        aria-hidden
        strokeWidth={1.5}
      />
      <h2 className="text-md font-semibold text-text-primary mb-2">{titulo}</h2>
      <p className="text-sm text-text-secondary mb-6 max-w-[40ch]">{detalle}</p>
      {onReintentar ? (
        <button
          type="button"
          onClick={onReintentar}
          className="h-9 px-4 rounded-md border border-border-strong bg-bg-surface text-text-primary text-sm font-medium hover:bg-bg-subtle focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus-ring"
        >
          Reintentar
        </button>
      ) : null}
    </div>
  )
}
