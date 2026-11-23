import { useId, type InputHTMLAttributes, type ReactNode } from 'react'
import type { FieldError, UseFormRegisterReturn } from 'react-hook-form'
import { clasesControl } from './clases'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface CampoTextoProps extends Omit<InputHTMLAttributes<HTMLInputElement>, keyof UseFormRegisterReturn> {
  etiqueta: string
  /** La etiqueta solo la leen los lectores de pantalla (p. ej. filas de opciones con su número visible). */
  etiquetaOculta?: boolean
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
export function CampoTexto({ etiqueta, etiquetaOculta, registro, error, ayuda, required, children, className = '', ...props }: CampoTextoProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const mensaje = error?.message

  return (
    <div className={`grid content-start gap-2 ${className}`}>
      {etiquetaOculta ? (
        <label htmlFor={id} className="sr-only">
          {etiqueta}
        </label>
      ) : (
        <Etiqueta htmlFor={id} required={required}>
          {etiqueta}
        </Etiqueta>
      )}
      <input
        id={id}
        aria-invalid={mensaje ? true : undefined}
        aria-describedby={mensaje || ayuda ? idMensaje : undefined}
        aria-required={required || undefined}
        className={clasesControl(Boolean(mensaje))}
        {...registro}
        {...props}
      />
      <MensajeCampo id={idMensaje} error={mensaje} ayuda={ayuda} />
      {children}
    </div>
  )
}
