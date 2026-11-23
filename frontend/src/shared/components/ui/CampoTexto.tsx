import { CircleAlert } from 'lucide-react'
import { useId, type InputHTMLAttributes, type ReactNode } from 'react'
import type { FieldError, UseFormRegisterReturn } from 'react-hook-form'

interface CampoTextoProps extends Omit<InputHTMLAttributes<HTMLInputElement>, keyof UseFormRegisterReturn> {
  etiqueta: string
  /** Marca el campo con * (la regla real está en el esquema Zod; no se usa la validación nativa). */
  required?: boolean
  /** Resultado de `register('campo')` de React Hook Form. */
  registro: UseFormRegisterReturn
  error?: FieldError
  /** Texto de ayuda bajo el campo; se oculta mientras hay un error. */
  ayuda?: ReactNode
  /** Contenido extra bajo el campo (p. ej. requisitos de contraseña). */
  children?: ReactNode
}

/**
 * Label + input + ayuda/error (design-system.md § Estados de componentes, Input).
 * Celular: 44px de alto y texto de 16px (evita el zoom del iPhone); escritorio: 36px y 14px.
 */
export function CampoTexto({ etiqueta, registro, error, ayuda, required, children, className = '', ...props }: CampoTextoProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const mensaje = error?.message

  return (
    <div className={`grid gap-2 ${className}`}>
      <label htmlFor={id} className="text-sm font-medium text-text-primary">
        {etiqueta}
        {required && (
          <span aria-hidden className="text-error-text">
            {' *'}
          </span>
        )}
      </label>
      <input
        id={id}
        aria-invalid={mensaje ? true : undefined}
        aria-describedby={mensaje || ayuda ? idMensaje : undefined}
        aria-required={required || undefined}
        className={[
          'h-control-touch w-full rounded-md border bg-bg-surface px-3 text-md text-text-primary transition-[border-color,box-shadow] sm:h-control sm:text-sm',
          'placeholder:text-text-tertiary focus-visible:outline-none focus:ring-3',
          'disabled:cursor-not-allowed disabled:border-border-default disabled:bg-bg-disabled disabled:text-text-disabled',
          mensaje
            ? 'border-error-icon focus:ring-error-bg'
            : 'border-border-input hover:border-border-input-hover focus:border-focus-ring focus:ring-focus-halo',
        ].join(' ')}
        {...registro}
        {...props}
      />
      {mensaje ? (
        <p id={idMensaje} className="flex items-start gap-1 text-xs text-error-text">
          <span aria-hidden className="flex h-lh shrink-0 items-center text-error-icon">
            <CircleAlert size={14} strokeWidth={1.5} />
          </span>
          {mensaje}
        </p>
      ) : (
        ayuda && (
          <p id={idMensaje} className="text-xs text-text-secondary">
            {ayuda}
          </p>
        )
      )}
      {children}
    </div>
  )
}
