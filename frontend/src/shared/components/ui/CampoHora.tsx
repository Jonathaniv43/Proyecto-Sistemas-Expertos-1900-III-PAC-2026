import { Clock } from 'lucide-react'
import { useId, type ReactNode } from 'react'
import { horaATexto, leerHora } from '@/shared/validacion'
import { clasesControl } from './clases'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface CampoHoraProps {
  etiqueta: string
  required?: boolean
  valor: string
  onChange: (valor: string) => void
  onBlur?: () => void
  error?: string
  ayuda?: ReactNode
}

/**
 * Hora (design-system.md § Campo de fecha, "Hora del accidente"): campo aparte, en 12 horas con a. m. / p. m., que se
 * escribe sin selector de reloj. Acepta "2:30 pm", "230p" o "14:30" y al salir del campo lo deja como "02:30 p. m.".
 */
export function CampoHora({ etiqueta, required, valor, onChange, onBlur, error, ayuda }: CampoHoraProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  return (
    <div className="grid content-start gap-2">
      <Etiqueta htmlFor={id} required={required}>
        {etiqueta}
      </Etiqueta>
      <div className="relative">
        <input
          id={id}
          type="text"
          autoComplete="off"
          placeholder="hh:mm a. m."
          maxLength={12}
          value={valor}
          aria-invalid={error ? true : undefined}
          aria-describedby={error || ayuda ? idMensaje : undefined}
          aria-required={required || undefined}
          className={`${clasesControl(Boolean(error))} pr-10 tabular-nums`}
          onChange={(e) => onChange(e.target.value)}
          onBlur={() => {
            const hora = leerHora(valor)
            if (hora) onChange(horaATexto(hora))
            onBlur?.()
          }}
        />
        <Clock aria-hidden size={16} strokeWidth={1.5} className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-text-secondary" />
      </div>
      <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />
    </div>
  )
}
