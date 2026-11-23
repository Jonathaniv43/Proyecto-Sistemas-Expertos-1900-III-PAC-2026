import { SearchCheck, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import type { ErrorApi } from '@/shared/api'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerPeligrosSugeridos } from '../api/encuestas'
import type { EstadoRiesgo, PeligrosSugeridos as Datos } from '../types/encuestas'

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
const formatoPorcentaje = new Intl.NumberFormat('es-HN', { maximumFractionDigits: 1 })

/** Badge del riesgo que registró la sugerencia (design-system.md § Badges: peligro sugerido). */
function EstadoSugerencia({ estado }: { estado: EstadoRiesgo }) {
  if (estado === 'confirmado') return <Insignia tipo="exito">En la matriz</Insignia>
  return <Insignia tipo="neutro">{estado === 'descartado' ? 'Descartado' : 'Por revisar'}</Insignia>
}

/**
 * HU-033: peligros que sugieren las respuestas (RN-13). Un peligro aparece cuando al menos una
 * respuesta eligió una opción que indica exposición en una pregunta asociada a ese peligro. Cada
 * sugerencia muestra cuántas respuestas la respaldan y de qué preguntas salió.
 *
 * Tiene su propia carga: si falla, el resto de los resultados se sigue viendo.
 */
export function PeligrosSugeridos({ encuestaId }: { encuestaId: string }) {
  const peticion = usePeticion(obtenerPeligrosSugeridos)
  const cargandoVisible = useCargaVisible(peticion.cargando)
  const { ejecutar } = peticion
  const cargar = useCallback(() => {
    void ejecutar(encuestaId)
  }, [ejecutar, encuestaId])
  useEffect(cargar, [cargar])

  return (
    <section
      aria-labelledby="titulo-peligros-sugeridos"
      className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
    >
      <header className="grid gap-1">
        <h2 id="titulo-peligros-sugeridos" className="text-md font-semibold">
          Peligros sugeridos
        </h2>
        <p className="text-text-secondary">
          Salen de las respuestas que indican exposición a un peligro, en todas las versiones de la encuesta. Te sirven de
          punto de partida para la matriz de riesgos.
        </p>
      </header>
      <Contenido peticion={peticion} cargandoVisible={cargandoVisible} onReintentar={cargar} />
    </section>
  )
}

interface ContenidoProps {
  peticion: { cargando: boolean; datos: Datos | undefined; error: ErrorApi | undefined }
  cargandoVisible: boolean
  onReintentar: () => void
}

function Contenido({ peticion, cargandoVisible, onReintentar }: ContenidoProps) {
  if (peticion.error) {
    return (
      <div className="flex flex-col items-center gap-3 px-4 py-8 text-center">
        <TriangleAlert aria-hidden size={24} strokeWidth={1.5} className="text-error-icon" />
        <p className="text-text-secondary">No se pudieron cargar los peligros sugeridos. {peticion.error.mensaje}</p>
        <Boton variante="secundario" onClick={onReintentar}>
          Reintentar
        </Boton>
      </div>
    )
  }

  const datos = peticion.datos
  if (peticion.cargando || !datos) {
    return (
      <div aria-busy="true" aria-label="Cargando peligros sugeridos" className="grid gap-3">
        {cargandoVisible &&
          [0, 1].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
      </div>
    )
  }

  if (datos.detalleOculto) {
    return (
      <p className="text-text-secondary">
        Las sugerencias aparecen con {datos.minimoRespuestas} respuestas, para proteger el anonimato. Van{' '}
        {plural(datos.totalRespuestas, 'respuesta', 'respuestas')}.
      </p>
    )
  }

  if (datos.sugerencias.length === 0) {
    return (
      <div className="flex flex-col items-center gap-2 px-4 py-8 text-center">
        <SearchCheck aria-hidden size={24} strokeWidth={1.5} className="text-text-tertiary" />
        <p className="font-medium">Ninguna respuesta indica exposición a un peligro</p>
        <p className="text-text-secondary">Si llegan respuestas nuevas que lo indiquen, el peligro aparecerá aquí.</p>
      </div>
    )
  }

  return (
    <ul className="grid gap-6" aria-label="Peligros sugeridos">
      {datos.sugerencias.map((s, i) => {
        const porcentaje = (s.respuestasConExposicion * 100) / datos.totalRespuestas
        return (
          <li key={s.riesgoSugeridoId} className={`grid gap-3 ${i > 0 ? 'border-t border-border-default pt-6' : ''}`}>
            <div className="flex flex-wrap items-center justify-between gap-2">
              <h3 className="font-semibold">{s.peligroNombre}</h3>
              <EstadoSugerencia estado={s.estado} />
            </div>
            <div className="grid gap-2">
              <p className="text-text-secondary">
                Lo indican {s.respuestasConExposicion} de {plural(datos.totalRespuestas, 'respuesta', 'respuestas')} ·{' '}
                <span className="tabular-nums">{formatoPorcentaje.format(porcentaje)} %</span>
              </p>
              <span aria-hidden className="flex h-2 w-full overflow-hidden rounded-full bg-dato-pista">
                <span className="h-full rounded-full bg-dato-serie" style={{ width: `${Math.max(Math.min(porcentaje, 100), 1)}%` }} />
              </span>
            </div>
            <div className="grid gap-1">
              <p className="text-xs text-text-secondary">
                {s.preguntas.length === 1 ? 'Pregunta que lo originó' : 'Preguntas que lo originaron'}
              </p>
              <ul className="grid gap-2 sm:gap-1">
                {s.preguntas.map((p) => (
                  <li key={p.preguntaId} className="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between sm:gap-4">
                    <span>{p.texto}</span>
                    <span className="shrink-0 text-sm whitespace-nowrap text-text-secondary tabular-nums">
                      {plural(p.respuestasConExposicion, 'respuesta', 'respuestas')}
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          </li>
        )
      })}
    </ul>
  )
}
