import type { ReporteEncuesta } from '../types/encuestas'

type Distribucion = NonNullable<ReporteEncuesta['preguntas'][number]['distribucion']>

const formatoPorcentaje = new Intl.NumberFormat('es-HN', { maximumFractionDigits: 1 })

/**
 * Distribución de respuestas de una pregunta de selección como "lista de medidores": por opción, su
 * nombre, una barra y la cifra. Las opciones conservan su orden (es el de la encuesta), un solo tono
 * de datos y sin leyenda (una sola serie). Toda cifra está impresa: no depende de un tooltip.
 * En pantallas angostas la barra baja debajo del nombre.
 */
export function DistribucionPregunta({ distribucion }: { distribucion: Distribucion }) {
  return (
    <ul className="grid gap-1">
      {distribucion.map((o) => {
        const porcentaje = `${formatoPorcentaje.format(o.porcentaje)} %`
        return (
          <li
            key={o.opcionId}
            tabIndex={0}
            aria-label={`${o.etiqueta}: ${o.cantidad} ${o.cantidad === 1 ? 'respuesta' : 'respuestas'}, ${porcentaje}`}
            className="grid gap-1 rounded-sm px-2 py-2 transition-colors hover:bg-bg-subtle focus-visible:bg-bg-subtle sm:flex sm:items-center sm:gap-4"
          >
            <span aria-hidden className="flex items-baseline justify-between gap-4 sm:w-1/3 sm:shrink-0">
              <span>{o.etiqueta}</span>
              <span className="whitespace-nowrap text-text-secondary tabular-nums sm:hidden">
                {o.cantidad} · {porcentaje}
              </span>
            </span>
            <span aria-hidden className="flex h-2 w-full overflow-hidden rounded-full bg-dato-pista sm:flex-1">
              {o.cantidad > 0 && (
                <span
                  className="h-full rounded-full bg-dato-serie"
                  style={{ width: `${Math.max(Math.min(o.porcentaje, 100), 1)}%` }}
                />
              )}
            </span>
            <span aria-hidden className="hidden shrink-0 text-right whitespace-nowrap text-text-secondary tabular-nums sm:block sm:w-1/6">
              {o.cantidad} · {porcentaje}
            </span>
          </li>
        )
      })}
    </ul>
  )
}
