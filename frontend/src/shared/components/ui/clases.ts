/** Clases de Tailwind compartidas por los componentes de ui/ (estados del design system). */

export type VarianteBoton = 'primario' | 'secundario' | 'peligro'

const VARIANTES: Record<VarianteBoton, string> = {
  primario: 'bg-action-primary text-text-on-brand hover:bg-action-primary-hover active:bg-action-primary-pressed',
  secundario:
    'border-border-strong bg-bg-surface text-text-primary hover:bg-bg-subtle active:bg-bg-pressed disabled:border-border-default',
  peligro: 'bg-action-danger text-white hover:bg-action-danger-hover active:bg-action-danger-pressed',
}

/** Clases del botón, para usarlas también en enlaces que se ven como botón (p. ej. <Link>). */
export const clasesBoton = (variante: VarianteBoton = 'primario') =>
  [
    'inline-flex h-control-touch items-center justify-center gap-2 whitespace-nowrap rounded-md border border-transparent px-4 text-sm font-medium no-underline transition-colors sm:h-control',
    VARIANTES[variante],
  ].join(' ')

/**
 * Clases del control de texto (input, select y área de texto): estados del design system.
 * `altoFijo: false` para el área de texto, que crece con el contenido.
 */
export const clasesControl = (conError: boolean, { altoFijo = true } = {}) =>
  [
    altoFijo ? 'h-control-touch sm:h-control' : '',
    'w-full rounded-md border bg-bg-surface px-3 text-md text-text-primary transition-[border-color,box-shadow] sm:text-sm',
    'placeholder:text-text-tertiary focus-visible:outline-none focus:ring-3',
    'disabled:cursor-not-allowed disabled:border-border-default disabled:bg-bg-disabled disabled:text-text-disabled',
    conError
      ? 'border-error-icon focus:ring-error-bg'
      : 'border-border-input hover:border-border-input-hover focus:border-focus-ring focus:ring-focus-halo',
  ].join(' ')
