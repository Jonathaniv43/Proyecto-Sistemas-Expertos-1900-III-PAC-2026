import { clasesBoton, type VarianteBoton } from './clases'
import type { ButtonHTMLAttributes } from 'react'


interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: VarianteBoton
  /** Muestra el spinner, usa `textoCargando` (gerundio) y bloquea el botón. */
  cargando?: boolean
  /** Texto mientras carga, en gerundio: "Guardando…", "Ingresando…". */
  textoCargando?: string
}

/** Botón del design system. Un solo botón primario por pantalla o sección. */
export function Boton({
  variante = 'primario',
  cargando = false,
  textoCargando,
  disabled,
  className = '',
  children,
  type = 'button',
  ...props
}: BotonProps) {
  return (
    <button
      type={type}
      disabled={disabled || cargando}
      aria-busy={cargando || undefined}
      className={[
        clasesBoton(variante),
        cargando
          ? 'cursor-progress'
          : 'disabled:cursor-not-allowed disabled:bg-bg-disabled disabled:text-text-disabled',
        className,
      ].join(' ')}
      {...props}
    >
      {cargando && (
<span aria-hidden className="spinner" />
      )}
      {cargando && textoCargando ? textoCargando : children}
    </button>
  )
}
