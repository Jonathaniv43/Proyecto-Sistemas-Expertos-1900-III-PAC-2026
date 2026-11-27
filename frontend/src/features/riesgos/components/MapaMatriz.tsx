import { CLASES_NIVEL, clasificar, ETIQUETAS_CLASIFICACION_RIESGO } from '../niveles'
import type { MatrizRiesgos } from '../types/riesgos'

const ESCALA = [1, 2, 3, 4, 5]
const plural = (n: number) => `${n} ${n === 1 ? 'riesgo' : 'riesgos'}`

interface MapaMatrizProps {
  matriz: MatrizRiesgos
  /** Celda elegida para filtrar la lista, o null. */
  seleccion: { probabilidad: number; severidad: number } | null
  onSeleccionar?: (celda: { probabilidad: number; severidad: number } | null) => void
  /** En impresión/PDF (HU-075), las celdas muestran el conteo sin botones interactivos. */
  soloLectura?: boolean
}

/** Leyenda de niveles y rangos de probabilidad × severidad (RN-16), legible también en blanco y negro. */
export function LeyendaNiveles() {
  return (
    <ul className="flex flex-wrap gap-x-4 gap-y-2 text-xs text-text-secondary" aria-label="Niveles">
      {(['bajo', 'medio', 'alto', 'critico'] as const).map((n, i) => (
        <li key={n} className="inline-flex items-center gap-2">
          <span aria-hidden className={`cuadro-riesgo ${CLASES_NIVEL[n]}`} />
          {ETIQUETAS_CLASIFICACION_RIESGO[n]} {['1–4', '5–9', '10–14', '15–25'][i]}
        </li>
      ))}
    </ul>
  )
}

/**
 * Mapa de la matriz (HU-038, design-system.md § Mapa de calor, "Ver"): cada celda muestra cuántos
 * riesgos tienen esa probabilidad y severidad en su última evaluación. Una celda con riesgos es un
 * botón que filtra la lista a ellos; tocarla otra vez quita el filtro.
 */
export function MapaMatriz({ matriz, seleccion, onSeleccionar, soloLectura = false }: MapaMatrizProps) {
  const cantidad = (p: number, s: number) => matriz.celdas.find((c) => c.probabilidad === p && c.severidad === s)?.cantidad ?? 0

  return (
    <div className="mapa-calor">
      <span className="mapa-calor-titulo-y" aria-hidden>
        Probabilidad
      </span>
      <div className="mapa-calor-celdas" role="group" aria-label="Riesgos por probabilidad y severidad">
        {[...ESCALA].reverse().map((p) => [
          <span key={`p${p}`} aria-hidden className="mapa-calor-eje">
            {p}
          </span>,
          ...ESCALA.map((s) => {
            const n = cantidad(p, s)
            const clasificacion = clasificar(p * s)
            const clases = `mapa-calor-celda ${CLASES_NIVEL[clasificacion]}`
            if (n === 0) return <span key={`${p}-${s}`} aria-hidden className={`${clases} mapa-calor-vacia`} />
            if (soloLectura) {
              return (
                <span
                  key={`${p}-${s}`}
                  aria-label={`Probabilidad ${p}, severidad ${s}: ${plural(n)}, ${ETIQUETAS_CLASIFICACION_RIESGO[clasificacion].toLowerCase()}`}
                  className={clases}
                >
                  {n}
                </span>
              )
            }
            const elegida = seleccion?.probabilidad === p && seleccion.severidad === s
            return (
              <button
                key={`${p}-${s}`}
                type="button"
                aria-pressed={elegida}
                aria-label={`Probabilidad ${p}, severidad ${s}: ${plural(n)}, ${ETIQUETAS_CLASIFICACION_RIESGO[clasificacion].toLowerCase()}`}
                className={clases}
                onClick={() => onSeleccionar?.(elegida ? null : { probabilidad: p, severidad: s })}
              >
                {n}
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
