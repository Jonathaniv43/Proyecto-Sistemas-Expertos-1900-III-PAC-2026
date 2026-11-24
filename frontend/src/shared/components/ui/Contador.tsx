interface ContadorProps {
  id?: string
  actual: number
  /** Máximo (área de texto) o largo exacto (RTN, DNI). */
  limite: number
  /** `true` para largo exacto: completo pasa a texto secundario. */
  exacto?: boolean
  /** Qué se cuenta, para lectores de pantalla. */
  unidad?: 'caracteres' | 'dígitos'
}

/**
 * Contador "120 / 500" (design-system.md § Controles de selección y área de texto): Caption terciario
 * con cifras tabulares, abajo a la derecha; al pasarse, en rojo. Solo anuncia al llegar al límite o
 * pasarse, no en cada tecla.
 */
export function Contador({ id, actual, limite, exacto = false, unidad = 'caracteres' }: ContadorProps) {
  const pasado = actual > limite
  const completo = actual === limite
  const color = pasado ? 'text-error-text' : exacto && completo ? 'text-text-secondary' : 'text-text-tertiary'
  const anuncio = pasado
    ? `Sobran ${actual - limite} ${unidad}.`
    : completo
      ? exacto
        ? `${limite} ${unidad}, completo.`
        : `Llegaste al máximo de ${limite} ${unidad}.`
      : ''

  return (
    <span id={id} className={`shrink-0 text-xs tabular-nums ${color}`}>
      <span aria-hidden>
        {actual} / {limite}
      </span>
      <span className="sr-only">
        {actual} de {limite} {unidad}
      </span>
      <span className="sr-only" aria-live="polite">
        {anuncio}
      </span>
    </span>
  )
}
