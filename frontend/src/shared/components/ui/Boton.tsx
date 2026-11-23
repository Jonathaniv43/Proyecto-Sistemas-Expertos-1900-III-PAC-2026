import type { ButtonHTMLAttributes } from 'react'

type Variante = 'primario' | 'secundario' | 'peligro'

interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: Variante
  /** Muestra el spinner, usa `textoCargando` (gerundio) y bloquea el botón. */
  cargando?: boolean
  /** Texto mientras carga, en gerundio: "Guardando…", "Ingresando…". */
  textoCargando?: string
}

const VARIANTES: Record<Variante, string> = {
  primario: 'bg-action-primary text-text-on-brand hover:bg-action-primary-hover active:bg-action-primary-pressed',
  secundario:
    'border-border-strong bg-bg-surface text-text-primary hover:bg-bg-subtle active:bg-bg-pressed disabled:border-border-default',
  peligro: 'bg-action-danger text-white hover:bg-action-danger-hover active:bg-action-danger-pressed',
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
        'inline-flex h-control-touch items-center justify-center gap-2 whitespace-nowrap rounded-md border border-transparent px-4 text-sm font-medium transition-colors sm:h-control',
        VARIANTES[variante],
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
