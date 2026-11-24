import { ArrowLeft, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { useWatch } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { evaluarRiesgo, evaluarRiesgoResidual, obtenerRiesgo } from '../api/riesgos'
import { EtiquetaRiesgo } from '../components/EtiquetaRiesgo'
import { MapaCalor } from '../components/MapaCalor'
import { SeccionControles } from '../components/SeccionControles'
import { nombrePeligro } from '../etiquetas'
import { clasificar, ETIQUETAS_CLASIFICACION_RIESGO, PROBABILIDADES, SEVERIDADES } from '../niveles'
import { evaluacionEsquema } from '../schemas/evaluacion'
import type { ClasificacionRiesgo, Evaluacion, Riesgo } from '../types/riesgos'

const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const fechaHora = (iso: string) => new Date(iso).toLocaleString('es-HN', { dateStyle: 'medium', timeStyle: 'short' })
const exigeControl = (c: ClasificacionRiesgo) => c === 'alto' || c === 'critico'

/**
 * Un riesgo de la matriz: sus datos y su evaluación. HU-037: se elige probabilidad y severidad del 1 al
 * 5 con su explicación a la vista (o tocando una celda del mapa), se ve el nivel calculado y queda quién
 * evaluó y cuándo. Volver a evaluar no borra la evaluación anterior (RN-20). HU-039: sus controles. HU-040:
 * la evaluación residual, al lado de la inicial.
 */
export function RiesgoPage() {
  const { id = '' } = useParams()
  const peticion = usePeticion(obtenerRiesgo)
  const cargandoVisible = useCargaVisible(peticion.cargando)
  const [riesgo, setRiesgo] = useState<Riesgo>()

  const { ejecutar } = peticion
  const cargar = useCallback(() => {
    void ejecutar(id).then((datos) => datos && setRiesgo(datos))
  }, [ejecutar, id])
  useEffect(cargar, [cargar])

  if (peticion.error?.status === 404) return <NotFoundPage />

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <div>
        <Link to="/riesgos" className="inline-flex items-center gap-1 text-sm">
          <ArrowLeft aria-hidden size={16} strokeWidth={1.5} />
          Volver a la matriz
        </Link>
      </div>

      {peticion.error && !riesgo ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar el riesgo</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !riesgo ? (
        <div aria-busy="true" aria-label="Cargando el riesgo" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          <header className="grid gap-3">
            <div className="grid gap-1">
              <h1 className="text-xl font-semibold">{nombrePeligro(riesgo)}</h1>
              {!riesgo.peligroId && <p className="text-xs text-text-secondary">Peligro descrito por la empresa</p>}
            </div>
            <dl className="grid gap-x-6 gap-y-2 sm:grid-cols-3">
              <Dato etiqueta="Actividad">{riesgo.actividad ?? '—'}</Dato>
              <Dato etiqueta="Origen">
                {riesgo.encuestaId ? (
                  <>
                    Encuesta{' '}
                    <Link to={`/encuestas/${riesgo.encuestaId}/resultados`}>{riesgo.encuestaTitulo ?? 'sin título'}</Link>
                  </>
                ) : (
                  'Agregado a mano'
                )}
              </Dato>
              <Dato etiqueta="Agregado">{fecha(riesgo.creadoEn)}</Dato>
            </dl>
          </header>

          {riesgo.estado !== 'confirmado' ? (
            <Aviso tipo="info" titulo="Este riesgo no está en la matriz">
              {riesgo.estado === 'descartado'
                ? 'Se descartó al revisar los peligros sugeridos de la encuesta, así que no se evalúa.'
                : 'Todavía hay que confirmarlo desde los peligros sugeridos de la encuesta; después se puede evaluar.'}
            </Aviso>
          ) : (
            <>
              <SeccionEvaluacion
                riesgo={riesgo}
                onEvaluado={(e) => setRiesgo((r) => r && (e.tipo === 'residual' ? { ...r, evaluacionResidual: e } : { ...r, evaluacion: e }))}
              />
              <SeccionControles riesgo={riesgo} onCambio={(totalControles) => setRiesgo((r) => r && { ...r, totalControles })} />
            </>
          )}
        </>
      )}
    </div>
  )
}

function Dato({ etiqueta, children }: { etiqueta: string; children: ReactNode }) {
  return (
    <div className="grid gap-1">
      <dt className="text-xs text-text-secondary">{etiqueta}</dt>
      <dd>{children}</dd>
    </div>
  )
}

type TipoEvaluacion = Evaluacion['tipo']

/** Qué pasó del nivel inicial al residual. */
function comparar(inicial: Evaluacion, residual: Evaluacion) {
  const de = `${ETIQUETAS_CLASIFICACION_RIESGO[inicial.clasificacion]} · ${inicial.nivel}`
  const a = `${ETIQUETAS_CLASIFICACION_RIESGO[residual.clasificacion]} · ${residual.nivel}`
  if (residual.nivel < inicial.nivel) return `Con los controles, el riesgo bajó de ${de} a ${a}.`
  if (residual.nivel > inicial.nivel) return `Con los controles, el riesgo subió de ${de} a ${a}. Revisa los controles.`
  return `Con los controles, el riesgo sigue en ${a}. Revisa los controles.`
}

/**
 * Evaluación del riesgo. HU-037: la inicial. HU-040: la residual, con los controles aplicados, separada de la
 * inicial y a su lado para ver si el riesgo bajó. La residual pide la inicial y al menos un control.
 */
function SeccionEvaluacion({ riesgo, onEvaluado }: { riesgo: Riesgo; onEvaluado: (e: Evaluacion) => void }) {
  const [evaluando, setEvaluando] = useState<TipoEvaluacion | null>(riesgo.evaluacion ? null : 'inicial')
  const [guardada, setGuardada] = useState<Evaluacion>()
  const inicial = riesgo.evaluacion
  const residual = riesgo.evaluacionResidual

  const abrir = (tipo: TipoEvaluacion) => {
    setGuardada(undefined)
    setEvaluando(tipo)
  }

  return (
    <section
      aria-labelledby="titulo-evaluacion"
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
    >
      <header className="grid gap-1">
        <h2 id="titulo-evaluacion" className="text-md font-semibold">
          Evaluación
        </h2>
        <p className="text-text-secondary">
          El nivel es probabilidad por severidad, cada una del 1 al 5: de 1 a 4 es bajo, de 5 a 9 medio, de 10 a 14 alto y
          de 15 a 25 crítico.
        </p>
      </header>

      <div role="status" className="contents">
        {guardada && !evaluando && (
          <Aviso tipo="exito">
            {`Evaluación ${guardada.tipo === 'residual' ? 'residual ' : ''}guardada: ${ETIQUETAS_CLASIFICACION_RIESGO[guardada.clasificacion]} · ${guardada.nivel}.`}
          </Aviso>
        )}
      </div>

      {evaluando || !inicial ? (
        <FormularioEvaluacion
          riesgoId={riesgo.id}
          tipo={evaluando ?? 'inicial'}
          actual={(evaluando === 'residual' ? residual : inicial) ?? null}
          onCancelar={inicial ? () => setEvaluando(null) : undefined}
          onGuardada={(evaluacion) => {
            onEvaluado(evaluacion)
            setGuardada(evaluacion)
            setEvaluando(null)
          }}
        />
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2">
            <PanelEvaluacion titulo="Inicial" descripcion="Antes de los controles." evaluacion={inicial}>
              <Boton variante="secundario" onClick={() => abrir('inicial')}>
                Volver a evaluar
              </Boton>
            </PanelEvaluacion>
            <PanelEvaluacion titulo="Residual" descripcion="Con los controles aplicados." evaluacion={residual}>
              {riesgo.totalControles === 0 ? (
                <p className="text-sm text-text-secondary">Agrega al menos un control para evaluar el riesgo residual.</p>
              ) : (
                <Boton variante="secundario" onClick={() => abrir('residual')}>
                  {residual ? 'Volver a evaluar' : 'Evaluar el residual'}
                </Boton>
              )}
            </PanelEvaluacion>
          </div>
          {residual && <p>{comparar(inicial, residual)}</p>}
        </>
      )}
    </section>
  )
}

interface PanelEvaluacionProps {
  titulo: string
  descripcion: string
  evaluacion: Evaluacion | null
  children: ReactNode
}

function PanelEvaluacion({ titulo, descripcion, evaluacion, children }: PanelEvaluacionProps) {
  return (
    <section aria-label={`Evaluación ${titulo.toLowerCase()}`} className="grid content-start gap-3 rounded-md border border-border-default p-4">
      <div className="grid gap-1">
        <h3 className="font-semibold">{titulo}</h3>
        <p className="text-xs text-text-secondary">{descripcion}</p>
      </div>
      {evaluacion ? (
        <div className="grid gap-2">
          <div>
            <EtiquetaRiesgo nivel={evaluacion} />
          </div>
          <p className="text-sm">
            Probabilidad {evaluacion.probabilidad} · {PROBABILIDADES[evaluacion.probabilidad - 1].nombre}
            <br />
            Severidad {evaluacion.severidad} · {SEVERIDADES[evaluacion.severidad - 1].nombre}
          </p>
          <p className="text-xs text-text-secondary">
            Evaluado por {evaluacion.evaluadoPorNombre} el {fechaHora(evaluacion.evaluadoEn)}
          </p>
        </div>
      ) : (
        <p className="text-sm text-text-secondary">Todavía no se evalúa.</p>
      )}
      <div>{children}</div>
    </section>
  )
}

function AvisoControl() {
  return (
    <p className="text-text-secondary">
      Los riesgos altos y críticos necesitan al menos un control para reducirlos.
    </p>
  )
}

interface FormularioEvaluacionProps {
  riesgoId: string
  tipo: TipoEvaluacion
  actual: Evaluacion | null
  onCancelar?: () => void
  onGuardada: (evaluacion: Evaluacion) => void
}

function FormularioEvaluacion({ riesgoId, tipo, actual, onCancelar, onGuardada }: FormularioEvaluacionProps) {
  const residual = tipo === 'residual'
  const {
    register,
    control,
    setValue,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(evaluacionEsquema, {
    // Al volver a evaluar se parte de la evaluación vigente del mismo tipo.
    defaultValues: {
      probabilidad: (actual ? String(actual.probabilidad) : '') as '1',
      severidad: (actual ? String(actual.severidad) : '') as '1',
    },
    erroresPorCodigo: {
      'riesgos.no_en_matriz': { mensaje: 'Este riesgo ya no está en la matriz. Vuelve a la matriz para ver su estado.' },
      'riesgos.sin_controles': { mensaje: 'El riesgo ya no tiene controles. Agrega uno para evaluar el residual.' },
      'riesgos.sin_evaluacion_inicial': { mensaje: 'Primero hay que hacer la evaluación inicial.' },
    },
  })
  const p = Number(useWatch({ control, name: 'probabilidad' })) || null
  const s = Number(useWatch({ control, name: 'severidad' })) || null
  const nivel = p && s ? { nivel: p * s, clasificacion: clasificar(p * s) } : null

  return (
    <form
      noValidate
      aria-label={residual ? 'Evaluación residual' : 'Evaluación inicial'}
      className="grid gap-6"
      onSubmit={enviar(async (datos) =>
        onGuardada(await (residual ? evaluarRiesgoResidual(riesgoId, datos) : evaluarRiesgo(riesgoId, datos))),
      )}
    >
      {residual && (
        <div className="grid gap-1">
          <h3 className="font-semibold">Evaluación residual</h3>
          <p className="text-text-secondary">
            Con los controles aplicados, ¿qué tan probable es ahora que el daño ocurra y qué tan grave sería? Se guarda
            aparte de la inicial.
          </p>
        </div>
      )}
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      {actual && (
        <p className="text-text-secondary">La evaluación anterior se conserva en el historial del riesgo.</p>
      )}

      <div className="grid gap-2">
        <p className="text-sm font-medium">Elige en el mapa</p>
        <p className="text-xs text-text-secondary">Tocar una celda elige la probabilidad y la severidad a la vez.</p>
        <MapaCalor
          probabilidad={p}
          severidad={s}
          onElegir={(prob, sev) => {
            setValue('probabilidad', String(prob) as '1', { shouldValidate: true, shouldDirty: true })
            setValue('severidad', String(sev) as '1', { shouldValidate: true, shouldDirty: true })
          }}
        />
      </div>

      <GrupoRadio
        etiqueta="Probabilidad"
        required
        ayuda="¿Qué tan probable es que el daño ocurra?"
        registro={register('probabilidad')}
        error={errors.probabilidad}
        opciones={PROBABILIDADES.map((o) => ({ valor: String(o.valor), texto: `${o.valor} · ${o.nombre}`, descripcion: o.explicacion }))}
      />
      <GrupoRadio
        etiqueta="Severidad"
        required
        ayuda="Si ocurre, ¿qué tan grave sería?"
        registro={register('severidad')}
        error={errors.severidad}
        opciones={SEVERIDADES.map((o) => ({ valor: String(o.valor), texto: `${o.valor} · ${o.nombre}`, descripcion: o.explicacion }))}
      />

      <div className="grid gap-2 rounded-md bg-bg-subtle p-4" aria-live="polite">
        <p className="text-sm font-medium">Nivel calculado</p>
        {nivel ? (
          <>
            <div>
              <EtiquetaRiesgo nivel={nivel} />
            </div>
            <p className="text-text-secondary">
              {p} × {s} = {nivel.nivel}
            </p>
            {!residual && exigeControl(nivel.clasificacion) && <AvisoControl />}
          </>
        ) : (
          <p className="text-text-secondary">Elige la probabilidad y la severidad para ver el nivel.</p>
        )}
      </div>

      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-end">
        {onCancelar && (
          <Boton variante="secundario" onClick={onCancelar} disabled={isSubmitting}>
            Cancelar
          </Boton>
        )}
        <Boton type="submit" cargando={isSubmitting} textoCargando="Guardando…">
          {residual ? 'Guardar evaluación residual' : 'Guardar evaluación'}
        </Boton>
      </div>
    </form>
  )
}
