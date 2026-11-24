const formatoPorcentaje = new Intl.NumberFormat('es-HN', { maximumFractionDigits: 1 })

export interface Medidor {
  clave: string
  etiqueta: string
  /** Aclaración debajo del nombre, en texto secundario (p. ej. "2 graves"). */
  detalle?: string
  cantidad: number
  /** De 0 a 100. */
  porcentaje: number
}

interface ListaMedidoresProps {
  filas: readonly Medidor[]
  /** Cómo se lee la cifra al anunciarla: ["respuesta", "respuestas"]. */
  unidad: readonly [string, string]
}

/**
 * Lista de medidores (design-system.md § Resultados de encuesta): por fila, su nombre, una barra y la cifra
 * "cantidad · %". Un solo tono de datos y sin leyenda (una sola serie); toda cifra está impresa, no depende de un
 * tooltip. Las filas conservan el orden que reciben. En pantallas angostas la barra baja debajo del nombre.
 */
export function ListaMedidores({ filas, unidad }: ListaMedidoresProps) {
  return (
    <ul className="grid gap-1">
      {filas.map((f) => {
        const porcentaje = `${formatoPorcentaje.format(f.porcentaje)} %`
        const cifra = `${f.cantidad} ${f.cantidad === 1 ? unidad[0] : unidad[1]}, ${porcentaje}`
        return (
          <li
            key={f.clave}
            tabIndex={0}
            aria-label={`${f.etiqueta}: ${cifra}${f.detalle ? `, ${f.detalle}` : ''}`}
            className="grid gap-1 rounded-sm px-2 py-2 transition-colors hover:bg-bg-subtle focus-visible:bg-bg-subtle sm:flex sm:items-center sm:gap-4"
          >
            <span aria-hidden className="flex items-baseline justify-between gap-4 sm:w-1/3 sm:shrink-0">
              <span className="grid">
                <span>{f.etiqueta}</span>
                {f.detalle && <span className="text-xs text-text-secondary">{f.detalle}</span>}
              </span>
              <span className="whitespace-nowrap text-text-secondary tabular-nums sm:hidden">
                {f.cantidad} · {porcentaje}
              </span>
            </span>
            <span aria-hidden className="flex h-2 w-full overflow-hidden rounded-full bg-dato-pista sm:flex-1">
              {f.cantidad > 0 && (
                <span className="h-full rounded-full bg-dato-serie" style={{ width: `${Math.max(Math.min(f.porcentaje, 100), 1)}%` }} />
              )}
            </span>
            <span aria-hidden className="hidden shrink-0 text-right whitespace-nowrap text-text-secondary tabular-nums sm:block sm:w-1/6">
              {f.cantidad} · {porcentaje}
            </span>
          </li>
        )
      })}
    </ul>
  )
}
