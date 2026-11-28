import { ArrowLeftRight, ChevronDown, ClipboardList, ShieldCheck, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useMemo } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { compararEncuestas, listarHistorialEncuestas } from '../api/encuestas'
import { DistribucionPregunta } from '../components/DistribucionPregunta'
import { EstadoEncuesta } from '../components/EstadoEncuesta'
import type {
  ComparacionEncuestas,
  EncuestaHistorial,
  EstadoRiesgo,
  LadoComparacion,
  PeligroComparado,
  PreguntaComparada,
} from '../types/encuestas'

const formatoPorcentaje = new Intl.NumberFormat('es-HN', { maximumFractionDigits: 1 })
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
const fechaCorta = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const fechaDateOnly = (iso: string) => {
  const [a, m, d] = iso.split('-').map(Number)
  return new Date(a, m - 1, d).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
}

interface OpcionVersion {
  versionId: string
  encuestaId: string
  texto: string
}

function aplanadoVersiones(lista: EncuestaHistorial[]): OpcionVersion[] {
  const opciones: OpcionVersion[] = []
  for (const encuesta of lista) {
    for (const v of encuesta.versiones) {
      const estado = v.estado === 'reemplazada' ? 'Anterior' : encuesta.estado === 'cerrada' ? 'Cerrada' : 'Vigente'
      const fechaTexto = v.publicadaEn ? ` · ${fechaCorta(v.publicadaEn)}` : ''
      opciones.push({
        versionId: v.id,
        encuestaId: encuesta.id,
        texto: `${encuesta.titulo} · v${v.numeroVersion} (${estado}${fechaTexto}) · ${plural(v.totalRespuestasAnonimas, 'resp.', 'resp.')}`,
      })
    }
  }
  return opciones
}

function DeltaPuntos({ delta }: { delta: number }) {
  if (Math.abs(delta) < 0.05) return <Insignia tipo="neutro">Sin cambio</Insignia>
  const signo = delta > 0 ? '+' : ''
  return (
    <Insignia tipo={delta > 0 ? 'info' : 'neutro'}>
      {`${signo}${formatoPorcentaje.format(delta)} pp`}
    </Insignia>
  )
}

function EstadoPeligro({ estado }: { estado: EstadoRiesgo | null }) {
  if (estado === 'confirmado') return <Insignia tipo="exito">En la matriz</Insignia>
  if (estado === 'descartado') return <Insignia tipo="neutro">Descartado</Insignia>
  if (estado === 'sugerido') return <Insignia tipo="neutro">Por revisar</Insignia>
  return null
}

/**
 * HU-034: compara resultados (preguntas, opciones, totales) y exposición a peligros sugeridos
 * entre dos versiones publicadas o cerradas (de la misma encuesta o de encuestas distintas),
 * respetando RN-12, D8 y RN-73.
 */
export function CompararEncuestasPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const baseParam = searchParams.get('base') ?? ''
  const compararParam = searchParams.get('comparar') ?? ''

  const candidatas = usePeticion(listarHistorialEncuestas)
  const comparacion = usePeticion(compararEncuestas)
  const cargandoListaVisible = useCargaVisible(candidatas.cargando && !candidatas.datos)

  const { ejecutar: cargarCandidatas } = candidatas
  useEffect(() => {
    void cargarCandidatas({ incluirVigentes: true })
  }, [cargarCandidatas])

  const opciones = useMemo(() => aplanadoVersiones(candidatas.datos ?? []), [candidatas.datos])

  // Si entran sin parámetros (o falta comparar), preselecciona dos versiones comparables por defecto.
  useEffect(() => {
    if (opciones.length < 2) return
    if (baseParam && compararParam) return

    const base = baseParam && opciones.some((o) => o.versionId === baseParam) ? baseParam : opciones[1].versionId
    const comp =
      compararParam && opciones.some((o) => o.versionId === compararParam) && compararParam !== base
        ? compararParam
        : (opciones.find((o) => o.versionId !== base)?.versionId ?? '')

    if (base && comp && (base !== baseParam || comp !== compararParam)) {
      setSearchParams({ base, comparar: comp }, { replace: true })
    }
  }, [opciones, baseParam, compararParam, setSearchParams])

  const mismoPeriodo = Boolean(baseParam && compararParam && baseParam === compararParam)
  const puedeComparar = Boolean(baseParam && compararParam && !mismoPeriodo)

  const { ejecutar: ejecutarComparacion } = comparacion
  const cargarComparacion = useCallback(() => {
    if (puedeComparar) void ejecutarComparacion(baseParam, compararParam)
  }, [puedeComparar, ejecutarComparacion, baseParam, compararParam])
  useEffect(cargarComparacion, [cargarComparacion])

  const cambiar = (lado: 'base' | 'comparar', valor: string) => {
    const siguiente = new URLSearchParams(searchParams)
    if (valor) siguiente.set(lado, valor)
    else siguiente.delete(lado)
    setSearchParams(siguiente, { replace: true })
  }

  const intercambiar = () => {
    if (!baseParam || !compararParam) return
    setSearchParams({ base: compararParam, comparar: baseParam }, { replace: true })
  }

  return (
    <div className="grid w-full max-w-layout gap-6">
      <Volver a="/encuestas/historial" texto="Volver a encuestas anteriores" titulo="Comparar resultados" ancho="layout" />

      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Comparar resultados</h1>
        <p className="flex items-start gap-2 text-text-secondary">
          <ShieldCheck aria-hidden size={16} strokeWidth={1.5} className="mt-1 shrink-0" />
          Compara cómo cambian las respuestas y la exposición a peligros entre encuestas o versiones distintas (RN-12, D8).
        </p>
      </header>

      {candidatas.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar las encuestas para comparar</h2>
          <p className="text-text-secondary">{candidatas.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargarCandidatas({ incluirVigentes: true })}>
            Reintentar
          </Boton>
        </section>
      ) : !candidatas.datos ? (
        <div aria-busy="true" aria-label="Cargando encuestas disponibles" className="grid gap-2">
          {cargandoListaVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : opciones.length < 2 ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ClipboardList aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <div className="grid gap-1">
            <h2 className="text-md font-semibold">Se necesitan al menos dos versiones publicadas para comparar</h2>
            <p className="text-text-secondary">
              Publica otra versión de una encuesta o crea una segunda encuesta y publícala para contrastar resultados.
            </p>
          </div>
          <Link to="/encuestas" className={clasesBoton('secundario')}>
            Ir a encuestas
          </Link>
        </section>
      ) : (
        <>
          <section
            aria-label="Selección de periodos a comparar"
            className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
          >
            <div className="grid items-end gap-4 md:grid-cols-[1fr_auto_1fr]">
              <div className="grid gap-2">
                <label htmlFor="select-base" className="text-sm font-medium">
                  Periodo base (A)
                </label>
                <div className="relative">
                  <select
                    id="select-base"
                    value={baseParam}
                    onChange={(e) => cambiar('base', e.target.value)}
                    className="min-h-control-touch w-full cursor-pointer appearance-none rounded-md border border-border-default bg-bg-surface pr-8 pl-3 text-sm text-text-primary focus-visible:border-border-focus focus-visible:outline-none sm:min-h-control"
                  >
                    <option value="" disabled>
                      Selecciona una encuesta o versión…
                    </option>
                    {opciones.map((o) => (
                      <option key={o.versionId} value={o.versionId}>
                        {o.texto}
                      </option>
                    ))}
                  </select>
                  <ChevronDown
                    aria-hidden
                    size={16}
                    strokeWidth={1.5}
                    className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-text-secondary"
                  />
                </div>
              </div>

              <div className="flex justify-center">
                <Boton variante="secundario" aria-label="Intercambiar periodo base y comparado" onClick={intercambiar} disabled={!puedeComparar}>
                  <ArrowLeftRight aria-hidden size={16} strokeWidth={1.5} />
                  Intercambiar
                </Boton>
              </div>

              <div className="grid gap-2">
                <label htmlFor="select-comparar" className="text-sm font-medium">
                  Comparar con (B)
                </label>
                <div className="relative">
                  <select
                    id="select-comparar"
                    value={compararParam}
                    onChange={(e) => cambiar('comparar', e.target.value)}
                    className="min-h-control-touch w-full cursor-pointer appearance-none rounded-md border border-border-default bg-bg-surface pr-8 pl-3 text-sm text-text-primary focus-visible:border-border-focus focus-visible:outline-none sm:min-h-control"
                  >
                    <option value="" disabled>
                      Selecciona una encuesta o versión…
                    </option>
                    {opciones.map((o) => (
                      <option key={o.versionId} value={o.versionId}>
                        {o.texto}
                      </option>
                    ))}
                  </select>
                  <ChevronDown
                    aria-hidden
                    size={16}
                    strokeWidth={1.5}
                    className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-text-secondary"
                  />
                </div>
              </div>
            </div>

            {mismoPeriodo && (
              <Aviso tipo="advertencia">
                Elegiste la misma versión en ambos lados. Selecciona dos encuestas o versiones distintas para compararlas.
              </Aviso>
            )}
          </section>

          {puedeComparar && (
            <>
              {comparacion.error && !comparacion.datos ? (
                <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
                  <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
                  <h2 className="text-md font-semibold">No se pudo generar la comparación</h2>
                  <p className="text-text-secondary">{comparacion.error.mensaje}</p>
                  <Boton variante="secundario" onClick={cargarComparacion}>
                    Reintentar
                  </Boton>
                </section>
              ) : !comparacion.datos ? (
                <div aria-busy="true" aria-label="Calculando comparación" className="grid gap-3">
                  {[0, 1, 2].map((i) => (
                    <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
                  ))}
                </div>
              ) : (
                <ContenidoComparacion datos={comparacion.datos} cargando={comparacion.cargando} />
              )}
            </>
          )}
        </>
      )}
    </div>
  )
}

function TarjetaLado({ etiqueta, lado, diferenciaRespuestas }: { etiqueta: string; lado: LadoComparacion; diferenciaRespuestas?: number }) {
  const periodo =
    lado.periodoDesde && lado.periodoHasta
      ? lado.periodoDesde === lado.periodoHasta
        ? fechaDateOnly(lado.periodoDesde)
        : `${fechaDateOnly(lado.periodoDesde)} – ${fechaDateOnly(lado.periodoHasta)}`
      : 'Sin respuestas registradas'

  return (
    <article className="grid content-start gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2">
          <Insignia tipo={etiqueta.includes('(B)') ? 'info' : 'neutro'}>{etiqueta}</Insignia>
          <EstadoEncuesta estado={lado.estadoEncuesta} />
          <Insignia tipo="neutro">Versión {lado.numeroVersion}</Insignia>
        </div>
        <Link to={`/encuestas/${lado.encuestaId}/resultados?version=${lado.versionId}`} className="text-sm">
          Ver resultados y peligros
        </Link>
      </div>

      <h2 className="text-md font-semibold">{lado.titulo}</h2>

      <div className="flex flex-wrap items-baseline gap-3">
        <span className="text-2xl font-semibold tabular-nums">{lado.totalRespuestasAnonimas}</span>
        <span className="text-sm text-text-secondary">
          {plural(lado.totalRespuestasAnonimas, 'respuesta en esta versión', 'respuestas en esta versión')}
          {diferenciaRespuestas !== undefined && diferenciaRespuestas !== 0 && (
            <> · {diferenciaRespuestas > 0 ? `+${diferenciaRespuestas}` : diferenciaRespuestas} respecto a A</>
          )}
        </span>
      </div>

      <dl className="grid gap-1 text-xs text-text-secondary">
        <div>
          <dt className="inline font-medium">Periodo: </dt>
          <dd className="inline">{periodo}</dd>
        </div>
        <div>
          <dt className="inline font-medium">Categorías: </dt>
          <dd className="inline">{lado.categorias.length > 0 ? lado.categorias.join(', ') : 'Sin categorías'}</dd>
        </div>
      </dl>
    </article>
  )
}

function ContenidoComparacion({ datos, cargando }: { datos: ComparacionEncuestas; cargando: boolean }) {
  const diffRespuestas = datos.comparar.totalRespuestasAnonimas - datos.base.totalRespuestasAnonimas
  const tieneBreves = datos.base.cantidadRespuestasBreves > 0 || datos.comparar.cantidadRespuestasBreves > 0

  return (
    <div className="grid gap-6" aria-busy={cargando}>
      <div className="grid gap-4 md:grid-cols-2">
        <TarjetaLado etiqueta="Base (A)" lado={datos.base} />
        <TarjetaLado etiqueta="Comparada (B)" lado={datos.comparar} diferenciaRespuestas={diffRespuestas} />
      </div>

      {datos.comparacionOculta ? (
        <Aviso tipo="info" titulo={`Se necesitan al menos ${datos.minimoRespuestas} respuestas en ambas versiones para comparar`}>
          Para proteger el anonimato (RN-73), no se comparan distribuciones ni peligros sugeridos mientras alguna versión tenga menos de{' '}
          {datos.minimoRespuestas} respuestas.
          {datos.base.detalleOculto &&
            ` ${datos.base.titulo} (v${datos.base.numeroVersion}) tiene ${plural(datos.base.totalRespuestasAnonimas, 'respuesta', 'respuestas')}.`}
          {datos.comparar.detalleOculto &&
            ` ${datos.comparar.titulo} (v${datos.comparar.numeroVersion}) tiene ${plural(datos.comparar.totalRespuestasAnonimas, 'respuesta', 'respuestas')}.`}
        </Aviso>
      ) : (
        <>
          <SeccionPeligrosComparados datos={datos} />
          <SeccionPreguntasComparadas datos={datos} />

          {tieneBreves && (
            <Aviso tipo="info" titulo="Preguntas de respuesta breve">
              Las preguntas de texto libre no se promedian en barras; puedes leer sus respuestas anónimas por separado en{' '}
              <Link to={`/encuestas/${datos.base.encuestaId}/resultados?version=${datos.base.versionId}`}>
                resultados de {datos.base.titulo} (v{datos.base.numeroVersion})
              </Link>{' '}
              y{' '}
              <Link to={`/encuestas/${datos.comparar.encuestaId}/resultados?version=${datos.comparar.versionId}`}>
                resultados de {datos.comparar.titulo} (v{datos.comparar.numeroVersion})
              </Link>
              .
            </Aviso>
          )}

          {(datos.preguntasSoloBase.length > 0 || datos.preguntasSoloComparar.length > 0) && (
            <section
              aria-labelledby="titulo-preguntas-exclusivas"
              className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
            >
              <header className="grid gap-1">
                <h2 id="titulo-preguntas-exclusivas" className="text-md font-semibold">
                  Preguntas sin par en el otro periodo
                </h2>
                <p className="text-text-secondary">
                  Preguntas de selección que estuvieron presentes solo en uno de los periodos comparados.
                </p>
              </header>

              {datos.preguntasSoloBase.length > 0 && (
                <div className="grid gap-4">
                  <h3 className="text-sm font-semibold text-text-secondary">
                    Solo en {datos.base.titulo} (v{datos.base.numeroVersion})
                  </h3>
                  {datos.preguntasSoloBase.map((p) => (
                    <article key={p.preguntaId} className="grid gap-2">
                      <p className="font-medium">{p.texto}</p>
                      {p.distribucion && <DistribucionPregunta distribucion={p.distribucion} />}
                    </article>
                  ))}
                </div>
              )}

              {datos.preguntasSoloComparar.length > 0 && (
                <div className="grid gap-4">
                  <h3 className="text-sm font-semibold text-text-secondary">
                    Solo en {datos.comparar.titulo} (v{datos.comparar.numeroVersion})
                  </h3>
                  {datos.preguntasSoloComparar.map((p) => (
                    <article key={p.preguntaId} className="grid gap-2">
                      <p className="font-medium">{p.texto}</p>
                      {p.distribucion && <DistribucionPregunta distribucion={p.distribucion} />}
                    </article>
                  ))}
                </div>
              )}
            </section>
          )}
        </>
      )}
    </div>
  )
}

function SeccionPeligrosComparados({ datos }: { datos: ComparacionEncuestas }) {
  const mismaEncuesta = datos.base.encuestaId === datos.comparar.encuestaId

  return (
    <section
      aria-labelledby="titulo-peligros-comparados"
      className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
    >
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div className="grid gap-1">
          <h2 id="titulo-peligros-comparados" className="text-md font-semibold">
            Peligros sugeridos y exposición
          </h2>
          <p className="text-text-secondary">
            Porcentaje de respuestas con exposición a cada peligro en ambos periodos.
          </p>
        </div>
        <div className="flex flex-wrap gap-3 text-sm">
          <Link to={`/encuestas/${datos.base.encuestaId}/resultados`}>Sugerencias en A</Link>
          {!mismaEncuesta && <Link to={`/encuestas/${datos.comparar.encuestaId}/resultados`}>Sugerencias en B</Link>}
        </div>
      </header>

      {datos.peligrosComparados.length === 0 ? (
        <p className="text-text-secondary">Ninguna de las dos versiones tiene respuestas que indiquen exposición a peligros.</p>
      ) : (
        <ul className="grid gap-5" aria-label="Comparación de peligros">
          {datos.peligrosComparados.map((p: PeligroComparado, i: number) => (
            <li key={p.peligroId} className={`grid gap-3 ${i > 0 ? 'border-t border-border-default pt-5' : ''}`}>
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="font-semibold">{p.peligroNombre}</h3>
                  <EstadoPeligro estado={p.estadoEnEncuestaComparar ?? p.estadoEnEncuestaBase} />
                </div>
                <DeltaPuntos delta={p.diferenciaPuntosPorcentuales} />
              </div>

              <div className="grid gap-2">
                <BarraComparativa
                  etiqueta={`Base (A · v${datos.base.numeroVersion})`}
                  cantidad={p.respuestasBase}
                  total={datos.base.totalRespuestasAnonimas}
                  porcentaje={p.porcentajeBase}
                  claseBarra="bg-text-tertiary"
                />
                <BarraComparativa
                  etiqueta={`Comparada (B · v${datos.comparar.numeroVersion})`}
                  cantidad={p.respuestasComparar}
                  total={datos.comparar.totalRespuestasAnonimas}
                  porcentaje={p.porcentajeComparar}
                  claseBarra="bg-dato-serie"
                />
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

function SeccionPreguntasComparadas({ datos }: { datos: ComparacionEncuestas }) {
  return (
    <section
      aria-labelledby="titulo-preguntas-comparadas"
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
    >
      <header className="grid gap-2">
        <h2 id="titulo-preguntas-comparadas" className="text-md font-semibold">
          Resultados por pregunta
        </h2>
        <div className="flex flex-wrap items-center gap-4 text-xs text-text-secondary">
          <span className="inline-flex items-center gap-2">
            <span aria-hidden className="inline-block h-2 w-4 rounded-full bg-text-tertiary" />
            Base (A): {datos.base.titulo} · v{datos.base.numeroVersion}
          </span>
          <span className="inline-flex items-center gap-2">
            <span aria-hidden className="inline-block h-2 w-4 rounded-full bg-dato-serie" />
            Comparada (B): {datos.comparar.titulo} · v{datos.comparar.numeroVersion}
          </span>
        </div>
      </header>

      {datos.preguntasComparadas.length === 0 ? (
        <p className="text-text-secondary">
          No hay preguntas de selección equivalentes entre ambos periodos. Revisa las preguntas individuales más abajo.
        </p>
      ) : (
        <div className="grid gap-8">
          {datos.preguntasComparadas.map((p: PreguntaComparada, i: number) => {
            const cambioTexto = p.textoBase.trim().toLowerCase() !== p.textoComparar.trim().toLowerCase()
            return (
              <article key={`${p.textoComparar}-${i}`} className={`grid gap-3 ${i > 0 ? 'border-t border-border-default pt-6' : ''}`}>
                <div className="grid gap-1">
                  <h3 className="text-md font-semibold">
                    {i + 1}. {p.textoComparar}
                  </h3>
                  {cambioTexto && (
                    <p className="text-xs text-text-secondary">En base (A): «{p.textoBase}»</p>
                  )}
                  <p className="text-xs text-text-secondary tabular-nums">
                    A: {plural(p.respondieronBase, 'persona respondió', 'personas respondieron')} · B:{' '}
                    {plural(p.respondieronComparar, 'persona respondió', 'personas respondieron')}
                    {p.tipoRespuesta === 'seleccion_multiple' && ' · selección múltiple (pueden sumar > 100 %)'}
                  </p>
                </div>

                <ul className="grid gap-4">
                  {p.opciones.map((o) => (
                    <li key={o.etiqueta} className="grid gap-2 rounded-sm bg-bg-subtle/50 p-3">
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        <span className="font-medium">{o.etiqueta}</span>
                        <DeltaPuntos delta={o.diferenciaPuntosPorcentuales} />
                      </div>
                      <div className="grid gap-1">
                        <BarraComparativa
                          etiqueta="A"
                          cantidad={o.cantidadBase}
                          porcentaje={o.porcentajeBase}
                          claseBarra="bg-text-tertiary"
                        />
                        <BarraComparativa
                          etiqueta="B"
                          cantidad={o.cantidadComparar}
                          porcentaje={o.porcentajeComparar}
                          claseBarra="bg-dato-serie"
                        />
                      </div>
                    </li>
                  ))}
                </ul>
              </article>
            )
          })}
        </div>
      )}
    </section>
  )
}

function BarraComparativa({
  etiqueta,
  cantidad,
  total,
  porcentaje,
  claseBarra,
}: {
  etiqueta: string
  cantidad: number
  total?: number
  porcentaje: number
  claseBarra: string
}) {
  const pctTexto = `${formatoPorcentaje.format(porcentaje)} %`
  const cifra = total !== undefined ? `${cantidad} de ${total} · ${pctTexto}` : `${cantidad} · ${pctTexto}`

  return (
    <div
      tabIndex={0}
      aria-label={`${etiqueta}: ${cifra}`}
      className="grid gap-1 rounded-sm px-1 py-1 sm:flex sm:items-center sm:gap-3"
    >
      <span className="text-xs text-text-secondary sm:w-44 sm:shrink-0">{etiqueta}</span>
      <span aria-hidden className="flex h-2 w-full overflow-hidden rounded-full bg-dato-pista sm:flex-1">
        {cantidad > 0 && (
          <span className={`h-full rounded-full ${claseBarra}`} style={{ width: `${Math.max(Math.min(porcentaje, 100), 1)}%` }} />
        )}
      </span>
      <span className="text-xs text-text-secondary tabular-nums sm:w-32 sm:shrink-0 sm:text-right">{cifra}</span>
    </div>
  )
}
