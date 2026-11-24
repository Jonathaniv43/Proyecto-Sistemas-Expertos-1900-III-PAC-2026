import { Plus, Search, ShieldAlert, TriangleAlert, X } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link, useLocation } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarRiesgos, obtenerMatriz } from '../api/riesgos'
import { EtiquetaRiesgo } from '../components/EtiquetaRiesgo'
import { MapaMatriz } from '../components/MapaMatriz'
import { ETIQUETAS_CLASIFICACION, nombrePeligro } from '../etiquetas'
import { CLASES_NIVEL, ETIQUETAS_CLASIFICACION_RIESGO } from '../niveles'
import type { ClasificacionPeligro, ClasificacionRiesgo, EstadoRiesgo, FiltrosRiesgos, Riesgo } from '../types/riesgos'

const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/** Lo que /riesgos/nuevo deja en el estado de la navegación al agregar un riesgo. */
export interface EstadoRiesgoAgregado {
  agregado?: string
}

type Celda = { probabilidad: number; severidad: number }
const FILTROS_VACIOS = { buscar: '', clasificacionPeligro: '', nivel: '', estado: 'confirmado' }
const NIVELES: ClasificacionRiesgo[] = ['critico', 'alto', 'medio', 'bajo']
const ESTADOS: { valor: EstadoRiesgo; texto: string }[] = [
  { valor: 'confirmado', texto: 'En la matriz' },
  { valor: 'sugerido', texto: 'Por revisar' },
  { valor: 'descartado', texto: 'Descartados' },
]

/**
 * Matriz de riesgos de la empresa (M4).
 * - HU-038: mapa de probabilidad × severidad con cuántos riesgos hay en cada celda (tocar una filtra la
 *   lista) y lista filtrable por peligro, clasificación del peligro, nivel y estado, con el nivel más
 *   alto primero para decidir qué atender. Cada riesgo abre su detalle.
 * - HU-036: agregar riesgos; HU-037: su nivel.
 */
export function RiesgosPage() {
  const matriz = usePeticion(obtenerMatriz)
  const riesgos = usePeticion(listarRiesgos)
  const cargandoVisible = useCargaVisible(riesgos.cargando && !riesgos.datos)
  const { state } = useLocation()
  const agregado = (state as EstadoRiesgoAgregado | null)?.agregado

  const { register, control, reset, setValue } = useForm({ defaultValues: FILTROS_VACIOS })
  const valores = useWatch({ control })
  const [celda, setCelda] = useState<Celda | null>(null)
  // La búsqueda espera a que se deje de escribir; los selects filtran al momento.
  const [buscar, setBuscar] = useState('')
  useEffect(() => {
    const t = setTimeout(() => setBuscar(valores.buscar?.trim() ?? ''), 300)
    return () => clearTimeout(t)
  }, [valores.buscar])

  const filtros = useMemo<FiltrosRiesgos>(() => {
    const f: FiltrosRiesgos = { estado: (valores.estado || 'confirmado') as EstadoRiesgo }
    if (buscar) f.buscar = buscar
    if (valores.clasificacionPeligro) f.clasificacionPeligro = valores.clasificacionPeligro as ClasificacionPeligro
    if (f.estado === 'confirmado') {
      if (valores.nivel === 'sin_evaluar') f.sinEvaluar = true
      else if (valores.nivel) f.nivel = valores.nivel as ClasificacionRiesgo
      if (celda) Object.assign(f, celda)
    }
    return f
  }, [valores.estado, valores.clasificacionPeligro, valores.nivel, buscar, celda])
  const hayFiltros = Boolean(buscar || valores.clasificacionPeligro || valores.nivel || celda || valores.estado !== 'confirmado')

  const { ejecutar: cargarMatriz } = matriz
  const { ejecutar: cargarRiesgos } = riesgos
  useEffect(() => {
    void cargarMatriz()
  }, [cargarMatriz])
  useEffect(() => {
    void cargarRiesgos(filtros)
  }, [cargarRiesgos, filtros])

  const quitarFiltros = () => {
    reset(FILTROS_VACIOS)
    setBuscar('')
    setCelda(null)
  }
  const elegirCelda = (c: Celda | null) => {
    // El mapa es de la matriz: elegir una celda vuelve a "En la matriz" y reemplaza el filtro de nivel.
    setValue('estado', 'confirmado')
    setValue('nivel', '')
    setCelda(c)
  }

  const sinRiesgos = matriz.datos?.total === 0 && !hayFiltros && riesgos.datos?.length === 0

  return (
    <div className="mx-auto grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Matriz de riesgos</h1>
          <p className="text-text-secondary">Dónde están los riesgos de la empresa y cuáles atender primero.</p>
        </div>
        <Link to="/riesgos/nuevo" className={clasesBoton('primario')}>
          <Plus aria-hidden size={16} strokeWidth={1.5} />
          Agregar riesgo
        </Link>
      </header>

      <div role="status" className="contents">
        {agregado && <Aviso tipo="exito">{`${agregado} quedó en la matriz.`}</Aviso>}
      </div>

      {riesgos.error && !riesgos.datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar la lista de riesgos</h2>
          <p className="text-text-secondary">{riesgos.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargarRiesgos(filtros)}>
            Reintentar
          </Boton>
        </section>
      ) : !riesgos.datos || !matriz.datos ? (
        <div aria-busy="true" aria-label="Cargando la matriz" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : sinRiesgos ? (
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
        <>
          <section
            aria-labelledby="titulo-mapa"
            className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6 lg:grid-cols-2 lg:items-center"
          >
            <div className="grid content-start gap-3">
              <h2 id="titulo-mapa" className="text-md font-semibold">
                Mapa de la matriz
              </h2>
              <p className="text-text-secondary">
                Cuántos riesgos hay en cada combinación de probabilidad y severidad, según su última evaluación. Toca una
                celda para ver sus riesgos en la lista.
              </p>
              <p>
                {plural(matriz.datos.total, 'riesgo en la matriz', 'riesgos en la matriz')}
                {matriz.datos.sinEvaluar > 0 && (
                  <>
                    {' · '}
                    <button
                      type="button"
                      className="font-medium text-text-link hover:text-text-link-hover"
                      onClick={() => {
                        setCelda(null)
                        setValue('estado', 'confirmado')
                        setValue('nivel', 'sin_evaluar')
                      }}
                    >
                      {plural(matriz.datos.sinEvaluar, 'sin evaluar', 'sin evaluar')}
                    </button>
                  </>
                )}
              </p>
              <ul className="flex flex-wrap gap-x-4 gap-y-2 text-xs text-text-secondary" aria-label="Niveles">
                {(['bajo', 'medio', 'alto', 'critico'] as const).map((n, i) => (
                  <li key={n} className="inline-flex items-center gap-2">
                    <span aria-hidden className={`cuadro-riesgo ${CLASES_NIVEL[n]}`} />
                    {ETIQUETAS_CLASIFICACION_RIESGO[n]} {['1–4', '5–9', '10–14', '15–25'][i]}
                  </li>
                ))}
              </ul>
            </div>
            <MapaMatriz matriz={matriz.datos} seleccion={celda} onSeleccionar={elegirCelda} />
          </section>

          <section aria-labelledby="titulo-lista" className="grid gap-4">
            <h2 id="titulo-lista" className="sr-only">
              Lista de riesgos
            </h2>
            <form
              role="search"
              aria-label="Filtrar riesgos"
              className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4"
              onSubmit={(e) => e.preventDefault()}
            >
              <CampoTexto etiqueta="Buscar" type="search" autoComplete="off" placeholder="Peligro o actividad" registro={register('buscar')} />
              <CampoSelect
                etiqueta="Clasificación del peligro"
                todos="Todas"
                registro={register('clasificacionPeligro')}
                opciones={Object.entries(ETIQUETAS_CLASIFICACION).map(([valor, texto]) => ({ valor, texto }))}
              />
              <CampoSelect
                etiqueta="Nivel"
                todos="Todos los niveles"
                disabled={valores.estado !== 'confirmado'}
                registro={register('nivel', { onChange: () => setCelda(null) })}
                opciones={[
                  ...NIVELES.map((n) => ({ valor: n, texto: ETIQUETAS_CLASIFICACION_RIESGO[n] })),
                  { valor: 'sin_evaluar', texto: 'Sin evaluar' },
                ]}
                ayuda={valores.estado !== 'confirmado' ? 'Solo los riesgos de la matriz tienen nivel.' : undefined}
              />
              <CampoSelect
                etiqueta="Estado"
                registro={register('estado', {
                  onChange: () => {
                    setValue('nivel', '')
                    setCelda(null)
                  },
                })}
                opciones={ESTADOS}
              />
            </form>

            {celda && (
              <div>
                <span className="inline-flex items-center gap-2 rounded-sm border border-border-default bg-bg-subtle py-1 pr-1 pl-2 text-sm">
                  Celda: probabilidad {celda.probabilidad} × severidad {celda.severidad}
                  <button
                    type="button"
                    aria-label="Quitar el filtro de la celda"
                    className="inline-flex rounded-sm p-1 text-text-secondary hover:bg-bg-pressed"
                    onClick={() => setCelda(null)}
                  >
                    <X aria-hidden size={14} strokeWidth={1.5} />
                  </button>
                </span>
              </div>
            )}

            {riesgos.datos.length === 0 ? (
              <div className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
                <Search aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
                <h3 className="text-md font-semibold">
                  {buscar ? `No hay resultados para «${buscar}»` : 'No hay riesgos con esos filtros'}
                </h3>
                <p className="text-text-secondary">Prueba con otra palabra o quita los filtros.</p>
                <button
                  type="button"
                  className="inline-flex min-h-control-touch items-center text-sm font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
                  onClick={quitarFiltros}
                >
                  Quitar filtros
                </button>
              </div>
            ) : (
              <div className="grid gap-2" aria-busy={riesgos.cargando}>
                <p className="text-xs text-text-secondary" aria-live="polite">
                  {plural(riesgos.datos.length, 'riesgo', 'riesgos')}
                  {valores.estado === 'confirmado' ? ', del nivel más alto al más bajo' : ''}
                </p>
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
          </section>
        </>
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

function Nivel({ riesgo }: { riesgo: Riesgo }) {
  if (riesgo.estado === 'sugerido') return <Insignia tipo="neutro">Por revisar</Insignia>
  if (riesgo.estado === 'descartado') return <Insignia tipo="neutro">Descartado</Insignia>
  return <EtiquetaRiesgo nivel={riesgo.evaluacion} />
}

function FilaRiesgo({ riesgo }: { riesgo: Riesgo }) {
  return (
    <tr>
      <td className="envolver">
        <Link to={`/riesgos/${riesgo.id}`} className="font-medium">
          {nombrePeligro(riesgo)}
        </Link>
        <div className="text-xs text-text-secondary">
          {riesgo.peligroClasificacion ? ETIQUETAS_CLASIFICACION[riesgo.peligroClasificacion] : 'Descrito por la empresa'}
        </div>
        {/* En celular, actividad y origen van aquí (design-system.md § Fila de tabla). */}
        <div className="mt-1 grid gap-1 text-sm sm:hidden">
          {riesgo.actividad && <span>{riesgo.actividad}</span>}
          <Origen riesgo={riesgo} />
        </div>
      </td>
      <td className="envolver max-sm:hidden">{riesgo.actividad ?? '—'}</td>
      <td>
        <Nivel riesgo={riesgo} />
      </td>
      <td className="envolver max-sm:hidden">
        <Origen riesgo={riesgo} />
      </td>
      <td className="tabular-nums text-text-secondary max-sm:hidden">{fecha(riesgo.creadoEn)}</td>
    </tr>
  )
}
