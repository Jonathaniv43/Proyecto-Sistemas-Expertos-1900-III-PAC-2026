import { ListChecks, Plus, Search, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useForm, useWatch, type UseFormReturn } from 'react-hook-form'
import { Link, useLocation } from 'react-router-dom'
import { extraerError } from '@/shared/api/errores'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { EtiquetaRiesgo } from '@/shared/riesgo/EtiquetaRiesgo'
import { fechaCorta } from '@/shared/validacion'
import { generarPlan, listarResponsables, listarTareas, obtenerPropuesta } from '../api/plan'
import { AsignarTareaModal } from '../components/AsignarTareaModal'
import { FiltrosPlan } from '../components/FiltrosPlan'
import { ESTADOS, PRIORIDADES, TIPOS_CONTROL, vencida } from '../etiquetas'
import { aFiltros, FILTROS_VACIOS, type ValoresFiltros } from '../filtros'
import type { FiltrosTareas, Responsable, Tarea, TareaPropuesta } from '../types/plan'

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/** Lo que /plan-de-trabajo/nueva deja en el estado de la navegación al crear una tarea. */
export interface EstadoPlanNavegacion {
  creada?: string
}

const cerrada = (t: Tarea) => t.estado === 'completada' || t.estado === 'cancelada'

/** De dónde sale la tarea, en palabras: el control que implementa o la tarea modelo del peligro. */
const origenPropuesta = (t: TareaPropuesta) =>
  t.origen === 'control' && t.tipoControl ? `Control · ${TIPOS_CONTROL[t.tipoControl]}` : 'Tarea modelo del peligro'

/**
 * HU-042: plan de trabajo. Norvia propone tareas a partir de los controles de cada riesgo evaluado y de las tareas
 * modelo de su peligro, de la prioridad más alta a la más baja. Se revisan, se desmarcan las que no se quieran
 * y se confirman; lo que ya tiene tarea no se vuelve a proponer, así que generarlo dos veces no duplica.
 * HU-043: cada tarea se asigna (responsable y fecha límite) y se crean tareas manuales.
 * HU-046: la lista se filtra por estado (o vencidas), responsable, origen, prioridad y fecha límite; las vencidas
 * llevan su badge y se cuentan junto a la lista, con un enlace que las filtra.
 */
export function PlanTrabajoPage() {
  const propuesta = usePeticion(obtenerPropuesta)
  const tareas = usePeticion(listarTareas)
  const cargandoVisible = useCargaVisible(!propuesta.datos || !tareas.datos)
  const responsables = usePeticion(listarResponsables)
  const [agregadas, setAgregadas] = useState<number>()
  const [asignando, setAsignando] = useState<Tarea>()
  const [asignada, setAsignada] = useState<string>()
  const { state } = useLocation()
  const creada = (state as EstadoPlanNavegacion | null)?.creada

  // Filtros (HU-046): la clave en texto evita volver a pedir la lista si los valores no cambiaron.
  const formFiltros = useForm<ValoresFiltros>({ defaultValues: FILTROS_VACIOS })
  const valores = useWatch({ control: formFiltros.control })
  const clave = JSON.stringify(aFiltros({ ...FILTROS_VACIOS, ...valores }))
  const filtros = useMemo<FiltrosTareas>(() => JSON.parse(clave), [clave])
  const hayFiltros = clave !== '{}'

  const { ejecutar: cargarPropuesta } = propuesta
  const { ejecutar: cargarTareas } = tareas
  const { ejecutar: cargarResponsables } = responsables
  const recargarTareas = useCallback(() => void cargarTareas(filtros), [cargarTareas, filtros])
  useEffect(recargarTareas, [recargarTareas])
  const cargar = useCallback(() => {
    void cargarPropuesta()
    void cargarResponsables()
    recargarTareas()
  }, [cargarPropuesta, cargarResponsables, recargarTareas])
  useEffect(() => {
    void cargarPropuesta()
    void cargarResponsables()
  }, [cargarPropuesta, cargarResponsables])

  const error = propuesta.error ?? tareas.error

  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Plan de trabajo</h1>
          <p className="text-text-secondary">Las tareas para reducir los riesgos de la empresa, empezando por las más urgentes.</p>
        </div>
        <Link to="/plan-de-trabajo/nueva" className={clasesBoton('secundario')}>
          <Plus aria-hidden size={16} strokeWidth={1.5} />
          Nueva tarea
        </Link>
      </header>

      <div role="status" className="contents">
        {asignada && <Aviso tipo="exito">{asignada}</Aviso>}
        {creada && !asignada && agregadas === undefined && <Aviso tipo="exito">{`La tarea «${creada}» quedó en el plan.`}</Aviso>}
        {agregadas !== undefined && !asignada && (
          <Aviso tipo="exito">
            {agregadas === 0
              ? 'Esas tareas ya estaban en el plan.'
              : `Se ${agregadas === 1 ? 'agregó 1 tarea' : `agregaron ${agregadas} tareas`} al plan.`}
          </Aviso>
        )}
      </div>

      {error && (!propuesta.datos || !tareas.datos) ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar el plan</h2>
          <p className="text-text-secondary">{error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !propuesta.datos || !tareas.datos ? (
        <div aria-busy="true" aria-label="Cargando el plan" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          <SeccionPropuesta
            key={propuesta.datos.tareas.map((t) => t.clave).join()}
            tareas={propuesta.datos.tareas}
            riesgosSinEvaluar={propuesta.datos.riesgosSinEvaluar}
            hayPlan={hayFiltros || tareas.datos.length > 0}
            onConfirmado={(n) => {
              setAsignada(undefined)
              setAgregadas(n)
              cargar()
            }}
          />
          <SeccionTareas
            tareas={tareas.datos}
            cargando={tareas.cargando}
            form={formFiltros}
            responsables={responsables.datos ?? []}
            hayFiltros={hayFiltros}
            puedeAsignar={responsables.datos !== undefined}
            onAsignar={(t) => {
              setAsignada(undefined)
              setAsignando(t)
            }}
          />
          <AsignarTareaModal
            tarea={asignando}
            responsables={responsables.datos ?? []}
            onCerrar={() => setAsignando(undefined)}
            onGuardada={(t) => {
              setAsignando(undefined)
              setAgregadas(undefined)
              setAsignada(`«${t.titulo}» quedó a cargo de ${t.responsableNombre ?? 'la persona elegida'}, para el ${fechaCorta(t.fechaLimite ?? '')}.`)
              recargarTareas()
            }}
          />
        </>
      )}
    </div>
  )
}

interface SeccionPropuestaProps {
  tareas: TareaPropuesta[]
  riesgosSinEvaluar: number
  hayPlan: boolean
  onConfirmado: (agregadas: number) => void
}

function SeccionPropuesta({ tareas, riesgosSinEvaluar, hayPlan, onConfirmado }: SeccionPropuestaProps) {
  // Todas marcadas al empezar: revisar es desmarcar lo que no se quiere.
  const [marcadas, setMarcadas] = useState<string[]>(() => tareas.map((t) => t.clave))
  const [enviando, setEnviando] = useState(false)
  const [errorEnvio, setErrorEnvio] = useState<string>()

  // Agrupadas por riesgo, en el orden de la propuesta (nivel más alto primero).
  const grupos = useMemo(() => {
    const porRiesgo = new Map<string, TareaPropuesta[]>()
    for (const t of tareas) porRiesgo.set(t.riesgoId, [...(porRiesgo.get(t.riesgoId) ?? []), t])
    return [...porRiesgo.values()]
  }, [tareas])

  const confirmar = async () => {
    setEnviando(true)
    setErrorEnvio(undefined)
    try {
      const { tareasCreadas } = await generarPlan(marcadas)
      onConfirmado(tareasCreadas)
    } catch (causa) {
      setErrorEnvio(extraerError(causa).mensaje)
    } finally {
      setEnviando(false)
    }
  }

  if (tareas.length === 0)
    return (
      <section aria-labelledby="titulo-propuesta" className="grid gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
        <h2 id="titulo-propuesta" className="text-md font-semibold">
          {hayPlan ? 'El plan está al día' : 'Todavía no hay tareas para proponer'}
        </h2>
        <p className="text-text-secondary">
          Las tareas salen de los controles de los riesgos evaluados y de las tareas modelo de su peligro. Cuando agregues
          controles en la <Link to="/riesgos">matriz de riesgos</Link>, aparecerán aquí para revisarlas.
        </p>
        {riesgosSinEvaluar > 0 && <AvisoSinEvaluar cantidad={riesgosSinEvaluar} />}
      </section>
    )

  return (
    <section aria-labelledby="titulo-propuesta" className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="grid gap-1">
        <h2 id="titulo-propuesta" className="text-md font-semibold">
          Tareas propuestas
        </h2>
        <p className="text-text-secondary">
          Salen de los controles de cada riesgo y de las tareas modelo de su peligro, de la prioridad más alta a la más
          baja. Revísalas y desmarca las que no quieras agregar: se volverán a proponer la próxima vez.
        </p>
      </header>

      {riesgosSinEvaluar > 0 && <AvisoSinEvaluar cantidad={riesgosSinEvaluar} />}
      {errorEnvio && <Aviso tipo="error">{errorEnvio}</Aviso>}

      <div className="grid gap-6">
        {grupos.map((grupo) => {
          const r = grupo[0]
          return (
            <GrupoCasillas
              key={r.riesgoId}
              etiqueta={
                <span className="flex flex-wrap items-center gap-2">
                  <Link to={`/riesgos/${r.riesgoId}`}>{r.riesgoNombre}</Link>
                  <EtiquetaRiesgo nivel={{ nivel: r.riesgoNivel, clasificacion: r.riesgoClasificacion }} />
                  <span className="text-xs font-normal text-text-secondary">Prioridad {PRIORIDADES[r.prioridad].toLowerCase()}</span>
                </span>
              }
              opciones={grupo.map((t) => ({
                valor: t.clave,
                texto: t.titulo,
                descripcion: [origenPropuesta(t), t.requiereEvidencia ? 'Pide evidencia' : null].filter(Boolean).join(' · '),
              }))}
              valor={marcadas.filter((c) => grupo.some((t) => t.clave === c))}
              onChange={(enGrupo) =>
                setMarcadas((m) => [...m.filter((c) => !grupo.some((t) => t.clave === c)), ...enGrupo])
              }
            />
          )
        })}
      </div>

      <div className="flex flex-col-reverse items-stretch gap-4 border-t border-border-default pt-4 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-text-secondary" aria-live="polite">
          {`${marcadas.length} de ${plural(tareas.length, 'tarea marcada', 'tareas marcadas')}`}
        </p>
        <Boton
          disabled={marcadas.length === 0}
          cargando={enviando}
          textoCargando="Agregando…"
          onClick={() => void confirmar()}
        >
          {marcadas.length === 0
            ? 'Marca al menos una tarea'
            : `Agregar ${plural(marcadas.length, 'tarea', 'tareas')} al plan`}
        </Boton>
      </div>
    </section>
  )
}

function AvisoSinEvaluar({ cantidad }: { cantidad: number }) {
  return (
    <Aviso tipo="info">
      {cantidad === 1
        ? '1 riesgo de la matriz no entra porque todavía no se evalúa: la prioridad sale de su nivel. '
        : `${cantidad} riesgos de la matriz no entran porque todavía no se evalúan: la prioridad sale de su nivel. `}
      <Link to="/riesgos">Ir a la matriz</Link>
    </Aviso>
  )
}

interface SeccionTareasProps {
  tareas: Tarea[]
  cargando: boolean
  form: UseFormReturn<ValoresFiltros>
  responsables: Responsable[]
  hayFiltros: boolean
  puedeAsignar: boolean
  onAsignar: (tarea: Tarea) => void
}

function SeccionTareas({ tareas, cargando, form, responsables, hayFiltros, puedeAsignar, onAsignar }: SeccionTareasProps) {
  const vencidas = tareas.filter(vencida).length
  const soloVencidas = form.watch('estado') === 'vencidas'
  return (
    <section aria-labelledby="titulo-tareas" className="grid gap-4">
      <h2 id="titulo-tareas" className="text-md font-semibold">
        Tareas del plan
      </h2>
      {(tareas.length > 0 || hayFiltros) && <FiltrosPlan form={form} responsables={responsables} />}
      {tareas.length === 0 && hayFiltros ? (
        <div className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <Search aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h3 className="text-md font-semibold">No hay tareas con esos filtros</h3>
          <p className="text-text-secondary">Prueba con otros o quita los filtros.</p>
          <button
            type="button"
            className="inline-flex min-h-control-touch items-center text-sm font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
            onClick={() => form.reset(FILTROS_VACIOS)}
          >
            Quitar filtros
          </button>
        </div>
      ) : tareas.length === 0 ? (
        <div className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ListChecks aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h3 className="text-md font-semibold">Todavía no hay tareas en el plan</h3>
          <p className="max-w-layout-narrow text-text-secondary">
            Confirma las tareas propuestas o crea una tarea nueva para empezar el plan.
          </p>
        </div>
      ) : (
        <div className="grid gap-2" aria-busy={cargando}>
          <p className="text-xs text-text-secondary" aria-live="polite">
            {plural(tareas.length, 'tarea', 'tareas')}, de la prioridad más alta a la más baja
            {vencidas > 0 && !soloVencidas && (
              <>
                {' · '}
                <button
                  type="button"
                  className="font-medium text-error-text hover:underline"
                  onClick={() => form.setValue('estado', 'vencidas')}
                >
                  {plural(vencidas, 'vencida', 'vencidas')}
                </button>
              </>
            )}
            {hayFiltros && (
              <>
                {' · '}
                <button type="button" className="font-medium text-text-link hover:text-text-link-hover" onClick={() => form.reset(FILTROS_VACIOS)}>
                  Quitar filtros
                </button>
              </>
            )}
          </p>
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Tarea</th>
                  <th scope="col" className="max-sm:hidden">
                    Prioridad
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Riesgo
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Estado
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Responsable
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Fecha límite
                  </th>
                  <th scope="col">
                    <span className="sr-only">Acciones</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {tareas.map((t) => (
                  <FilaTarea key={t.id} tarea={t} puedeAsignar={puedeAsignar} onAsignar={onAsignar} />
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  )
}

function OrigenTarea({ tarea }: { tarea: Tarea }) {
  if (tarea.origen === 'riesgo' && tarea.riesgoId)
    return (
      <span className="text-xs text-text-secondary">
        <Link to={`/riesgos/${tarea.riesgoId}`}>{tarea.riesgoNombre}</Link>
        {tarea.controlTipo ? ` · Control: ${TIPOS_CONTROL[tarea.controlTipo].toLowerCase()}` : ' · Tarea modelo'}
      </span>
    )
  if (tarea.origen === 'accidente' && tarea.accidenteId)
    return (
      <span className="text-xs text-text-secondary">
        <Link to={`/eventos/${tarea.accidenteId}`}>Medida de un evento</Link>
      </span>
    )
  return <span className="text-xs text-text-secondary">Manual</span>
}

function Estado({ tarea }: { tarea: Tarea }) {
  const estado = ESTADOS[tarea.estado]
  return (
    <span className="inline-flex flex-wrap gap-1">
      <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
      {vencida(tarea) && <Insignia tipo="error">Vencida</Insignia>}
    </span>
  )
}

function FilaTarea({ tarea, puedeAsignar, onAsignar }: { tarea: Tarea } & Pick<SeccionTareasProps, 'puedeAsignar' | 'onAsignar'>) {
  return (
    <tr>
      <td className="envolver">
        <div className="font-medium">{tarea.titulo}</div>
        <OrigenTarea tarea={tarea} />
        {/* En celular, prioridad, nivel, responsable, fecha y estado van aquí: así la acción queda a la vista (design-system.md § Fila de tabla). */}
        <div className="mt-1 grid justify-items-start gap-1 text-sm sm:hidden">
          <span>Prioridad {PRIORIDADES[tarea.prioridad].toLowerCase()}</span>
          {tarea.riesgoClasificacion && tarea.riesgoNivel !== null && (
            <EtiquetaRiesgo nivel={{ nivel: tarea.riesgoNivel, clasificacion: tarea.riesgoClasificacion }} />
          )}
          <span className="text-text-secondary">
            {tarea.responsableNombre ?? 'Sin responsable'}
            {tarea.fechaLimite && ` · hasta el ${fechaCorta(tarea.fechaLimite)}`}
          </span>
          <Estado tarea={tarea} />
        </div>
      </td>
      <td className="max-sm:hidden">{PRIORIDADES[tarea.prioridad]}</td>
      <td className="max-sm:hidden">
        {tarea.riesgoClasificacion && tarea.riesgoNivel !== null ? (
          <EtiquetaRiesgo nivel={{ nivel: tarea.riesgoNivel, clasificacion: tarea.riesgoClasificacion }} />
        ) : (
          <span className="text-text-tertiary">—</span>
        )}
      </td>
      <td className="max-sm:hidden">
        <Estado tarea={tarea} />
      </td>
      <td className="max-sm:hidden">
        {tarea.responsableNombre ?? <span className="text-text-secondary">Sin responsable</span>}
      </td>
      <td className="tabular-nums max-sm:hidden">
        {tarea.fechaLimite ? fechaCorta(tarea.fechaLimite) : <span className="text-text-tertiary">—</span>}
      </td>
      <td className="text-right">
        {!cerrada(tarea) && puedeAsignar && (
          <Boton variante="secundario" aria-label={`${tarea.responsableId ? 'Reasignar' : 'Asignar'} «${tarea.titulo}»`} onClick={() => onAsignar(tarea)}>
            {tarea.responsableId ? 'Reasignar' : 'Asignar'}
          </Boton>
        )}
      </td>
    </tr>
  )
}
