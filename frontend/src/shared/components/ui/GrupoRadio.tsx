import { useId, type ReactNode } from 'react'
import type { FieldError, UseFormRegisterReturn } from 'react-hook-form'
import { MensajeCampo } from './MensajeCampo'

interface GrupoRadioProps {
  etiqueta: string
  required?: boolean
  registro: UseFormRegisterReturn
  /** `descripcion` (opcional) se muestra bajo el texto, en gris. */
  opciones: readonly { valor: string; texto: string; descripcion?: string }[]
  error?: FieldError
  ayuda?: ReactNode
  className?: string
}

/**
 * Opciones en columna (design-system.md § Controles de selección). El error va una sola vez
 * debajo del grupo; las opciones no se ponen rojas. En celular cada opción mide 44px de alto.
 */
export function GrupoRadio({ etiqueta, required, registro, opciones, error, ayuda, className = '' }: GrupoRadioProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const mensaje = error?.message

  return (
    <fieldset
      className={`grid content-start gap-2 ${className}`}
      aria-invalid={mensaje ? true : undefined}
      aria-describedby={mensaje || ayuda ? idMensaje : undefined}
    >
      <legend className="mb-2 text-sm font-medium text-text-primary">
        {etiqueta}
        {required && (
          <span aria-hidden className="text-error-text">
            {' *'}
          </span>
        )}
      </legend>
      <div className="grid justify-items-start sm:gap-3">
        {opciones.map((opcion) => (
          <label
            key={opcion.valor}
            className={`radio min-h-control-touch sm:min-h-auto ${opcion.descripcion ? 'items-start max-sm:py-2' : ''}`}
          >
            <input type="radio" value={opcion.valor} {...registro} />
            {opcion.descripcion ? (
              <>
                <span className="flex h-lh items-center">
                  <span aria-hidden className="radio-circulo" />
                </span>
                <span className="grid">
                  {opcion.texto}
                  <span className="text-xs text-text-secondary">{opcion.descripcion}</span>
                </span>
              </>
            ) : (
              <>
                <span aria-hidden className="radio-circulo" />
                {opcion.texto}
              </>
            )}
          </label>
        ))}
      </div>
      <MensajeCampo id={idMensaje} error={mensaje} ayuda={ayuda} />
    </fieldset>
  )
}
