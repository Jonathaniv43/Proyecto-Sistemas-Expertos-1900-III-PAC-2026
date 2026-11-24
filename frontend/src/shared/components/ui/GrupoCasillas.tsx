import { Check } from 'lucide-react'
import { useId, type ReactNode } from 'react'
import { MensajeCampo } from './MensajeCampo'

export interface OpcionCasilla {
  valor: string
  texto: string
  descripcion?: ReactNode
}

interface GrupoCasillasProps {
  /** Texto de la leyenda; puede llevar más que texto (p. ej. el nivel de un riesgo en el plan). */
  etiqueta: ReactNode
  required?: boolean
  opciones: readonly OpcionCasilla[]
  /** Valores marcados (controlado: con React Hook Form, usar `useController`). */
  valor: readonly string[]
  onChange: (valor: string[]) => void
  onBlur?: () => void
  error?: string
  ayuda?: ReactNode
  className?: string
}

/**
 * Checkboxes en columna (design-system.md § Controles de selección). El error va una sola vez
 * debajo del grupo; las opciones no se ponen rojas. En celular cada opción mide al menos 44px.
 */
export function GrupoCasillas({ etiqueta, required, opciones, valor, onChange, onBlur, error, ayuda, className = '' }: GrupoCasillasProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`

  const alternar = (opcion: string, marcada: boolean) =>
    onChange(marcada ? [...valor, opcion] : valor.filter((v) => v !== opcion))

  return (
    <fieldset
      className={`grid content-start gap-2 ${className}`}
      aria-invalid={error ? true : undefined}
      aria-describedby={error || ayuda ? idMensaje : undefined}
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
            className={`casilla min-h-control-touch sm:min-h-auto ${opcion.descripcion ? 'items-start max-sm:py-2' : ''}`}
          >
            <input
              type="checkbox"
              value={opcion.valor}
              checked={valor.includes(opcion.valor)}
              onChange={(e) => alternar(opcion.valor, e.target.checked)}
              onBlur={onBlur}
            />
            <span className="flex h-lh items-center">
              <span aria-hidden className="casilla-caja">
                <Check strokeWidth={2.5} />
              </span>
            </span>
            <span className="grid">
              {opcion.texto}
              {opcion.descripcion && <span className="text-xs text-text-secondary">{opcion.descripcion}</span>}
            </span>
          </label>
        ))}
      </div>
      <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />
    </fieldset>
  )
}
