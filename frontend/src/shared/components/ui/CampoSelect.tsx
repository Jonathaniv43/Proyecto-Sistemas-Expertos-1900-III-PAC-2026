import { ChevronDown } from 'lucide-react'
import { useId, type ReactNode } from 'react'
import type { FieldError, UseFormRegisterReturn } from 'react-hook-form'
import { clasesControl } from './clases'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface CampoSelectProps {
  etiqueta: string
  required?: boolean
  registro: UseFormRegisterReturn
  opciones: readonly { valor: string; texto: string }[]
  error?: FieldError
  ayuda?: ReactNode
  disabled?: boolean
  className?: string
}

/**
 * Select cerrado como el input, con chevron y "Selecciona…" sin valor (design-system.md § Select).
 * Usa el select nativo: en celular el sistema abre su propia lista. Con 5 opciones o menos, usar GrupoRadio.
 */
export function CampoSelect({ etiqueta, required, registro, opciones, error, ayuda, disabled, className = '' }: CampoSelectProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const mensaje = error?.message

  return (
    <div className={`grid content-start gap-2 ${className}`}>
      <Etiqueta htmlFor={id} required={required}>
        {etiqueta}
      </Etiqueta>
      <div className="relative">
        <select
          id={id}
          disabled={disabled}
          aria-invalid={mensaje ? true : undefined}
          aria-describedby={mensaje || ayuda ? idMensaje : undefined}
          aria-required={required || undefined}
          className={`${clasesControl(Boolean(mensaje))} cursor-pointer appearance-none pr-8 invalid:text-text-tertiary`}
          required
          {...registro}
        >
          <option value="" disabled>
            Selecciona…
          </option>
          {opciones.map((opcion) => (
            <option key={opcion.valor} value={opcion.valor} className="text-text-primary">
              {opcion.texto}
            </option>
          ))}
        </select>
        <ChevronDown
          aria-hidden
          size={16}
          strokeWidth={1.5}
          className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-text-secondary"
        />
      </div>
      <MensajeCampo id={idMensaje} error={mensaje} ayuda={ayuda} />
    </div>
  )
}
