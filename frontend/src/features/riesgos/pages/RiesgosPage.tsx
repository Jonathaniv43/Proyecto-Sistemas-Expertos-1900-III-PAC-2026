import { Plus, ShieldAlert, TriangleAlert } from 'lucide-react'
import { useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarRiesgos } from '../api/riesgos'
import { EtiquetaRiesgo } from '../components/EtiquetaRiesgo'
import { nombrePeligro } from '../etiquetas'
import type { Riesgo } from '../types/riesgos'

const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/** Lo que /riesgos/nuevo deja en el estado de la navegación al agregar un riesgo. */
export interface EstadoRiesgoAgregado {
  agregado?: string
}

/**
 * Matriz de riesgos de la empresa (M4). HU-036: lista los riesgos confirmados, vengan de una encuesta
 * (HU-035) o agregados a mano, y lleva a agregar uno. La evaluación y el mapa de calor llegan con
 * HU-037 y HU-038.
 */
export function RiesgosPage() {
  const riesgos = usePeticion(listarRiesgos)
  const cargandoVisible = useCargaVisible(riesgos.cargando)
  const { state } = useLocation()
  const agregado = (state as EstadoRiesgoAgregado | null)?.agregado

  const { ejecutar: cargar } = riesgos
  useEffect(() => {
    void cargar()
  }, [cargar])

  return (
    <div className="mx-auto grid w-full max-w-layout gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Matriz de riesgos</h1>
          <p className="text-text-secondary">
            Los riesgos de la empresa: los que se confirmaron desde una encuesta y los que se agregaron a mano.
          </p>
        </div>
        <Link to="/riesgos/nuevo" className={clasesBoton('primario')}>
          <Plus aria-hidden size={16} strokeWidth={1.5} />
          Agregar riesgo
        </Link>
      </header>

      <div role="status" className="contents">
        {agregado && <Aviso tipo="exito">{`${agregado} quedó en la matriz.`}</Aviso>}
      </div>

      {riesgos.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar la lista de riesgos</h2>
          <p className="text-text-secondary">{riesgos.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar()}>
            Reintentar
          </Boton>
        </section>
      ) : riesgos.cargando || !riesgos.datos ? (
        <div aria-busy="true" aria-label="Cargando riesgos" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : riesgos.datos.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ShieldAlert aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">Todavía no hay riesgos identificados</h2>
          <p className="max-w-layout-narrow text-text-secondary">
            Confírmalos desde los peligros sugeridos en los resultados de una encuesta, o agrega los que la encuesta no
            detectó.
          </p>
          <Link to="/riesgos/nuevo" className={`${clasesBoton('primario')} mt-2`}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Agregar riesgo
          </Link>
        </section>
      ) : (
        <div className="grid gap-2">
          <p className="text-xs text-text-secondary">{plural(riesgos.datos.length, 'riesgo', 'riesgos')}</p>
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Peligro</th>
                  <th scope="col" className="max-sm:hidden">
                    Actividad
                  </th>
                  <th scope="col">Nivel</th>
                  <th scope="col" className="max-sm:hidden">
                    Origen
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Agregado
                  </th>
                </tr>
              </thead>
              <tbody>
                {riesgos.datos.map((r) => (
                  <FilaRiesgo key={r.id} riesgo={r} />
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}

function Origen({ riesgo }: { riesgo: Riesgo }) {
  if (!riesgo.encuestaId) return <span className="text-text-secondary">Agregado a mano</span>
  return (
    <span className="text-text-secondary">
      Encuesta <Link to={`/encuestas/${riesgo.encuestaId}/resultados`}>{riesgo.encuestaTitulo ?? 'sin título'}</Link>
    </span>
  )
}

function FilaRiesgo({ riesgo }: { riesgo: Riesgo }) {
  return (
    <tr>
      <td className="envolver">
        <div className="font-medium">{nombrePeligro(riesgo)}</div>
        {!riesgo.peligroId && <div className="text-xs text-text-secondary">Descrito por la empresa</div>}
        {/* En celular, actividad y origen van aquí (design-system.md § Fila de tabla). */}
        <div className="mt-1 grid gap-1 text-sm sm:hidden">
          {riesgo.actividad && <span>{riesgo.actividad}</span>}
          <Origen riesgo={riesgo} />
        </div>
      </td>
      <td className="envolver max-sm:hidden">{riesgo.actividad ?? '—'}</td>
      <td>
        <EtiquetaRiesgo />
      </td>
      <td className="envolver max-sm:hidden">
        <Origen riesgo={riesgo} />
      </td>
      <td className="tabular-nums text-text-secondary max-sm:hidden">{fecha(riesgo.creadoEn)}</td>
    </tr>
  )
}
