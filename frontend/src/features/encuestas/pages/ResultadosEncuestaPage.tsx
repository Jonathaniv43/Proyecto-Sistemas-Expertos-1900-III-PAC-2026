import { ArrowLeft, ChartColumn, ShieldCheck, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, type ReactNode } from 'react'
import { Link, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerEncuesta, obtenerReporte } from '../api/encuestas'
import { DistribucionPregunta } from '../components/DistribucionPregunta'
import { RespuestasBreves } from '../components/RespuestasBreves'
import type { EncuestaDetalle, ReporteEncuesta } from '../types/encuestas'

/** Mínimo de respuestas para ver el detalle (ObtenerReporteEncuestaServicio.MinimoParaDetalle, RN-73). */
const MINIMO_DETALLE = 3

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
// Las fechas del periodo vienen como 'AAAA-MM-DD' (DateOnly): se leen como fecha local, sin zona horaria.
const fecha = (iso: string) => {
  const [a, m, d] = iso.split('-').map(Number)
  return new Date(a, m - 1, d).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
}

/**
 * HU-032: resultados anónimos de la versión publicada vigente: participantes, respuestas, periodo,
 * distribución por pregunta y respuestas breves sin autor. Con menos de 3 respuestas solo se ven
 * los totales, para no identificar a nadie.
 */
export function ResultadosEncuestaPage() {
  const { id = '' } = useParams()
  const encuesta = usePeticion(obtenerEncuesta)
  const reporte = usePeticion(obtenerReporte)
  const cargandoVisible = useCargaVisible(encuesta.cargando || reporte.cargando)

  const { ejecutar: cargarEncuesta } = encuesta
  const { ejecutar: cargarReporte } = reporte
  const cargar = useCallback(() => {
    void cargarEncuesta(id)
    void cargarReporte(id)
  }, [cargarEncuesta, cargarReporte, id])
  useEffect(cargar, [cargar])

  if (encuesta.error?.status === 404) return <NotFoundPage />

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <div>
        <Link to={`/encuestas/${id}`} className="inline-flex items-center gap-1 text-sm">
          <ArrowLeft aria-hidden size={16} strokeWidth={1.5} />
          Volver a la encuesta
        </Link>
      </div>

      {encuesta.error || (reporte.error && reporte.error.codigo !== 'encuestas.sin_version_publicada') ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudieron cargar los resultados</h1>
          <p className="text-text-secondary">{(encuesta.error ?? reporte.error)!.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !encuesta.datos || (!reporte.datos && !reporte.error) ? (
        <div aria-busy="true" aria-label="Cargando resultados" className="grid gap-3">
          {cargandoVisible && [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : !reporte.datos ? (
        <SinPublicar titulo={encuesta.datos.titulo} />
      ) : (
        <Resultados encuesta={encuesta.datos} reporte={reporte.datos} />
      )}
    </div>
  )
}

function SinPublicar({ titulo }: { titulo: string }) {
  return (
    <>
      <h1 className="text-xl font-semibold">Resultados: {titulo}</h1>
      <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
        <ChartColumn aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
        <h2 className="text-md font-semibold">Todavía no hay resultados</h2>
        <p className="text-text-secondary">La encuesta no se ha publicado. Cuando los encuestados respondan, verás aquí sus resultados.</p>
      </section>
    </>
  )
}

function Indicador({ etiqueta, valor, detalle }: { etiqueta: string; valor: ReactNode; detalle?: string }) {
  return (
    <div className="grid content-start gap-1 rounded-lg border border-border-default bg-bg-surface p-4">
      <dt className="text-xs text-text-secondary">{etiqueta}</dt>
      <dd className="text-2xl font-semibold tabular-nums">{valor}</dd>
      {detalle && <dd className="text-xs text-text-secondary">{detalle}</dd>}
    </div>
  )
}

function Resultados({ encuesta, reporte }: { encuesta: EncuestaDetalle; reporte: ReporteEncuesta }) {
  const version = encuesta.versionPublicada?.numeroVersion
  const periodo =
    reporte.periodoDesde && reporte.periodoHasta
      ? reporte.periodoDesde === reporte.periodoHasta
        ? fecha(reporte.periodoDesde)
        : `${fecha(reporte.periodoDesde)} – ${fecha(reporte.periodoHasta)}`
      : null

  return (
    <>
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Resultados: {encuesta.titulo}</h1>
        <p className="flex items-start gap-2 text-text-secondary">
          <ShieldCheck aria-hidden size={16} strokeWidth={1.5} className="mt-1 shrink-0" />
          Los resultados son anónimos: no muestran quién respondió ni el orden en que se respondió.
        </p>
      </header>

      <dl className="grid gap-3 sm:grid-cols-3">
        <Indicador etiqueta="Participantes" valor={reporte.totalParticipaciones} detalle="Personas que respondieron, en todas las versiones" />
        <Indicador
          etiqueta="Respuestas"
          valor={reporte.totalRespuestasAnonimas}
          detalle={version ? `De la versión ${version}, la vigente` : 'De la versión vigente'}
        />
        <Indicador etiqueta="Periodo analizado" valor={<span className="text-md">{periodo ?? 'Sin respuestas'}</span>} detalle={periodo ? 'Primera y última respuesta' : undefined} />
      </dl>

      {reporte.detalleOculto ? (
        <Aviso tipo="info" titulo="Los resultados por pregunta aparecen con 3 respuestas">
          Para proteger el anonimato, no se muestra el detalle hasta que haya al menos {MINIMO_DETALLE} respuestas en la versión vigente.
          {` Van ${plural(reporte.totalRespuestasAnonimas, 'respuesta', 'respuestas')}.`}
        </Aviso>
      ) : (
        <section className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-label="Resultados por pregunta">
          {reporte.preguntas.map((p, i) => (
            <ResultadoPregunta key={p.preguntaId} numero={i + 1} pregunta={p} />
          ))}
        </section>
      )}
    </>
  )
}

function ResultadoPregunta({ numero, pregunta }: { numero: number; pregunta: ReporteEncuesta['preguntas'][number] }) {
  const titulo = (
    <h2 className="text-md font-semibold">
      {numero}. {pregunta.texto}
    </h2>
  )

  if (pregunta.textosBreves) {
    return (
      <article className="grid gap-3">
        {titulo}
        <p className="text-xs text-text-secondary">{plural(pregunta.textosBreves.length, 'respuesta escrita', 'respuestas escritas')}</p>
        <RespuestasBreves textos={pregunta.textosBreves} />
      </article>
    )
  }

  const distribucion = pregunta.distribucion ?? []
  const multiple = pregunta.tipoRespuesta === 'seleccion_multiple'
  // Quienes respondieron esta pregunta: base de los porcentajes del backend.
  const conValor = distribucion.find((o) => o.cantidad > 0)
  const respondieron = multiple
    ? conValor
      ? Math.round((conValor.cantidad * 100) / conValor.porcentaje)
      : 0
    : distribucion.reduce((suma, o) => suma + o.cantidad, 0)

  return (
    <article className="grid gap-3">
      {titulo}
      <p className="text-xs text-text-secondary">
        {plural(respondieron, 'persona respondió', 'personas respondieron')}
        {multiple && ' · se podía elegir más de una opción, así que los porcentajes pueden sumar más de 100 %'}
      </p>
      {respondieron === 0 ? <p className="text-text-secondary">Nadie respondió esta pregunta.</p> : <DistribucionPregunta distribucion={distribucion} />}
    </article>
  )
}
