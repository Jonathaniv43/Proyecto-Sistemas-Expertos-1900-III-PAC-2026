import { useId, type ReactNode, type Ref, type TextareaHTMLAttributes } from 'react'
import { useLargoControl } from '@/shared/hooks/useLargoControl'
import { clasesControl } from './clases'
import { Contador } from './Contador'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface AreaTextoProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  etiqueta: string
  required?: boolean
  error?: string
  ayuda?: ReactNode
  /** Límite de caracteres del esquema: muestra el contador "120 / 500". */
  maximo?: number
  ref?: Ref<HTMLTextAreaElement>
}

const contarCaracteres = (valor: string) => valor.length

/**
 * Área de texto del design system: mismos estados que el input, mínimo 3 líneas, crece hasta ~8 y
 * luego desplaza; sin tirador. En celular, texto de 16px. Con `maximo`, contador abajo a la derecha.
 */
export function AreaTexto({ etiqueta, required, error, ayuda, maximo, className = '', ref, ...props }: AreaTextoProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const idContador = `${id}-contador`
  const { largo, ref: refControl } = useLargoControl(contarCaracteres, ref)
  const describe = [error || ayuda ? idMensaje : '', maximo ? idContador : ''].filter(Boolean).join(' ')

  return (
    <div className={`grid content-start gap-2 ${className}`}>
      <Etiqueta htmlFor={id} required={required}>
        {etiqueta}
      </Etiqueta>
      <textarea
        id={id}
        ref={refControl}
        aria-invalid={error ? true : undefined}
        aria-describedby={describe || undefined}
        aria-required={required || undefined}
        className={`${clasesControl(Boolean(error), { altoFijo: false })} area-texto`}
        {...props}
      />
      {maximo ? (
        <div className="flex items-start justify-between gap-4">
          <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />
          <span className="ml-auto">
            <Contador id={idContador} actual={largo} limite={maximo} />
          </span>
        </div>
      ) : (
        <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />
      )}
    </div>
  )
}
