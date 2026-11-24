import { CLASES_NIVEL, clasificar, ETIQUETAS_CLASIFICACION_RIESGO } from '../niveles'

interface MapaCalorProps {
  probabilidad: number | null
  severidad: number | null
  /** Tocar una celda elige su probabilidad y su severidad a la vez. */
  onElegir: (probabilidad: number, severidad: number) => void
}

const ESCALA = [1, 2, 3, 4, 5]

/**
 * Mapa de calor de riesgo para evaluar (design-system.md § Mapa de calor): severidad en horizontal,
 * probabilidad en vertical (5 arriba). Cada celda muestra P × S con el color de su nivel; la elegida
 * lleva un anillo. Es un atajo: las listas de probabilidad y severidad, con su explicación, son el
 * control principal y quedan sincronizadas.
 */
export function MapaCalor({ probabilidad, severidad, onElegir }: MapaCalorProps) {
  return (
    <div className="mapa-calor">
      <span className="mapa-calor-titulo-y" aria-hidden>
        Probabilidad
      </span>
      <div className="mapa-calor-celdas" role="group" aria-label="Mapa de probabilidad por severidad">
        {[...ESCALA].reverse().map((p) => [
          <span key={`p${p}`} aria-hidden className="mapa-calor-eje">
            {p}
          </span>,
          ...ESCALA.map((s) => {
            const nivel = p * s
            const clasificacion = clasificar(nivel)
            return (
              <button
                key={`${p}-${s}`}
                type="button"
                aria-pressed={p === probabilidad && s === severidad}
                aria-label={`Probabilidad ${p}, severidad ${s}: nivel ${nivel}, ${ETIQUETAS_CLASIFICACION_RIESGO[clasificacion].toLowerCase()}`}
                className={`mapa-calor-celda ${CLASES_NIVEL[clasificacion]}`}
                onClick={() => onElegir(p, s)}
              >
                {nivel}
              </button>
            )
          }),
        ])}
        <span aria-hidden />
        {ESCALA.map((s) => (
          <span key={`s${s}`} aria-hidden className="mapa-calor-eje">
            {s}
          </span>
        ))}
      </div>
      <span className="mapa-calor-titulo-x" aria-hidden>
        Severidad
      </span>
    </div>
  )
}
