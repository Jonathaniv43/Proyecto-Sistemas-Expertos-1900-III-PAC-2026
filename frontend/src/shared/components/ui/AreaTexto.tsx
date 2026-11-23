import { useId, type ReactNode, type TextareaHTMLAttributes } from 'react'
import { clasesControl } from './clases'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface AreaTextoProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  etiqueta: string
  required?: boolean
  error?: string
  ayuda?: ReactNode
}

/**
 * Área de texto del design system: mismos estados que el input, mínimo 3 líneas, crece hasta ~8 y
 * luego desplaza; sin tirador. En celular, texto de 16px.
 */
export function AreaTexto({ etiqueta, required, error, ayuda, className = '', ...props }: AreaTextoProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  return (
    <div className={`grid content-start gap-2 ${className}`}>
      <Etiqueta htmlFor={id} required={required}>
        {etiqueta}
      </Etiqueta>
      <textarea
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || ayuda ? idMensaje : undefined}
        aria-required={required || undefined}
        className={`${clasesControl(Boolean(error), { altoFijo: false })} area-texto`}
        {...props}
      />
      <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />
    </div>
  )
}
