import { ArrowLeftRight, ChartColumn, ShieldCheck, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Indicador } from '@/shared/components/datos/Indicador'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerEncuesta, obtenerReporte } from '../api/encuestas'
import { DistribucionPregunta } from '../components/DistribucionPregunta'
import { PeligrosSugeridos } from '../components/PeligrosSugeridos'
import { RespuestasBreves } from '../components/RespuestasBreves'
import type { EncuestaDetalle, ReporteEncuesta, VersionEncuestaResumen } from '../types/encuestas'

/** Mínimo de respuestas para ver el detalle (AgregadorReporteVersion.MinimoParaDetalle, RN-73). */
const MINIMO_DETALLE = 3

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
// Las fechas del periodo vienen como 'AAAA-MM-DD' (DateOnly): se leen como fecha local, sin zona horaria.
const fecha = (iso: string) => {
  const [a, m, d] = iso.split('-').map(Number)
  return new Date(a, m - 1, d).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
}
const fechaHora = (iso: string) =>
  new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })

/**
 * HU-032: resultados anónimos de la versión publicada vigente: participantes, respuestas, periodo,
 * distribución por pregunta y respuestas breves sin autor. Con menos de 3 respuestas solo se ven
 * los totales, para no identificar a nadie.
 *
 * HU-033: entre los indicadores y el detalle van los peligros sugeridos por las respuestas.
 *
 * HU-034: permite cambiar entre versiones publicadas/reemplazadas mediante `?version=` y saltar a
 * comparar versiones o encuestas.
 */
export function ResultadosEncuestaPage() {
  const { id = '' } = useParams()
  const [searchParams, setSearchParams] = useSearchParams()
  const versionParam = searchParams.get('version') ?? undefined

  const encuesta = usePeticion(obtenerEncuesta)
  const reporte = usePeticion(obtenerReporte)
  const cargandoVisible = useCargaVisible(encuesta.cargando || (reporte.cargando && !reporte.datos))

  const { ejecutar: cargarEncuesta } = encuesta
  const { ejecutar: cargarReporte } = reporte
  const recargar = useCallback(() => {
    void cargarEncuesta(id)
    void cargarReporte(id, versionParam)
  }, [cargarEncuesta, cargarReporte, id, versionParam])

  useEffect(() => {
    void cargarEncuesta(id)
  }, [cargarEncuesta, id])

  useEffect(() => {
    void cargarReporte(id, versionParam)
  }, [cargarReporte, id, versionParam])

  if (encuesta.error?.status === 404 || reporte.error?.codigo === 'encuestas.version_no_encontrada') {
    return <NotFoundPage />
  }

  const errorBloqueante =
    encuesta.error ||
    (reporte.error && reporte.error.codigo !== 'encuestas.sin_version_publicada' ? reporte.error : undefined)

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a={`/encuestas/${id}`} texto="Volver a la encuesta" titulo={encuesta.datos?.titulo} />

      {errorBloqueante ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudieron cargar los resultados</h1>
          <p className="text-text-secondary">{errorBloqueante.mensaje}</p>
          <div className="flex flex-wrap justify-center gap-3">
            {versionParam && (
              <Boton variante="secundario" onClick={() => setSearchParams({}, { replace: true })}>
                Ver versión vigente
              </Boton>
            )}
            <Boton variante="secundario" onClick={recargar}>
              Reintentar
            </Boton>
          </div>
        </section>
      ) : !encuesta.datos || (!reporte.datos && !reporte.error) ? (
        <div aria-busy="true" aria-label="Cargando resultados" className="grid gap-3">
          {cargandoVisible && [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : !reporte.datos ? (
        <SinPublicar titulo={encuesta.datos.titulo} />
      ) : (
        <Resultados
          encuesta={encuesta.datos}
          reporte={reporte.datos}
          cargandoVersion={reporte.cargando}
          onCambiarVersion={(nuevaVersionId) => {
            const vigenteId = encuesta.datos?.versionPublicada?.id
            if (!nuevaVersionId || nuevaVersionId === vigenteId) setSearchParams({}, { replace: true })
            else setSearchParams({ version: nuevaVersionId }, { replace: true })
          }}
        />
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

function etiquetaVersion(v: VersionEncuestaResumen, encuestaCerrada: boolean) {
  const sufijoEstado = v.estado === 'reemplazada' ? 'Anterior' : encuestaCerrada ? 'Al cierre' : 'Vigente'
  const fechaTexto = v.publicadaEn ? ` · ${fechaHora(v.publicadaEn)}` : ''
  return `Versión ${v.numeroVersion} (${sufijoEstado}${fechaTexto}) · ${plural(v.totalRespuestasAnonimas, 'respuesta', 'respuestas')}`
}

function Resultados({
  encuesta,
  reporte,
  cargandoVersion,
  onCambiarVersion,
}: {
  encuesta: EncuestaDetalle
  reporte: ReporteEncuesta
  cargandoVersion: boolean
  onCambiarVersion: (versionId: string) => void
}) {
  const versiones = encuesta.versionesPublicadas ?? []
  const esHistorica = reporte.estadoVersion === 'reemplazada'
  const detalleVersion = esHistorica
    ? `De la versión ${reporte.numeroVersion} (reemplazada)`
    : encuesta.estado === 'cerrada'
      ? `De la versión ${reporte.numeroVersion} al cierre`
      : `De la versión ${reporte.numeroVersion}, la vigente`

  const periodo =
    reporte.periodoDesde && reporte.periodoHasta
      ? reporte.periodoDesde === reporte.periodoHasta
        ? fecha(reporte.periodoDesde)
        : `${fecha(reporte.periodoDesde)} – ${fecha(reporte.periodoHasta)}`
      : null

  const enlaceComparar =
    versiones.length >= 2
      ? `/encuestas/comparar?base=${versiones[1].id}&comparar=${versiones[0].id}`
      : `/encuestas/comparar?base=${reporte.versionId}`

  return (
    <div className="grid gap-6" aria-busy={cargandoVersion}>
      <header className="grid gap-2">
        <div className="flex flex-wrap items-center gap-2">
          <h1 className="text-xl font-semibold">Resultados: {encuesta.titulo}</h1>
          {encuesta.estado === 'cerrada' && <Insignia tipo="neutro">Cerrada</Insignia>}
          {esHistorica && <Insignia tipo="neutro">Versión {reporte.numeroVersion} (anterior)</Insignia>}
        </div>
        <p className="flex items-start gap-2 text-text-secondary">
          <ShieldCheck aria-hidden size={16} strokeWidth={1.5} className="mt-1 shrink-0" />
          Los resultados son anónimos: no muestran quién respondió ni el orden en que se respondió.
        </p>
      </header>

      <div className="flex flex-wrap items-end justify-between gap-4 rounded-lg border border-border-default bg-bg-surface p-4">
        {versiones.length > 1 ? (
          <div className="grid min-w-64 flex-1 gap-2">
            <label htmlFor="selector-version-resultados" className="text-sm font-medium">
              Versión consultada
            </label>
            <select
              id="selector-version-resultados"
              value={reporte.versionId}
              onChange={(e) => onCambiarVersion(e.target.value)}
              className="min-h-control-touch w-full rounded-md border border-border-default bg-bg-surface px-3 text-sm text-text-primary focus-visible:border-border-focus focus-visible:outline-none sm:min-h-control"
            >
              {versiones.map((v) => (
                <option key={v.id} value={v.id}>
                  {etiquetaVersion(v, encuesta.estado === 'cerrada')}
                </option>
              ))}
            </select>
          </div>
        ) : (
          <p className="text-sm text-text-secondary">
            Versión {reporte.numeroVersion}
            {encuesta.cerradaEn ? ` · cerrada el ${fechaHora(encuesta.cerradaEn)}` : ''}
          </p>
        )}

        <Link to={enlaceComparar} className={clasesBoton('secundario')}>
          <ArrowLeftRight aria-hidden size={16} strokeWidth={1.5} />
          {versiones.length >= 2 ? 'Comparar versiones' : 'Comparar con otra encuesta'}
        </Link>
      </div>

      <dl className="grid gap-3 sm:grid-cols-3">
        <Indicador etiqueta="Participantes" valor={reporte.totalParticipaciones} detalle="Personas que respondieron, en todas las versiones" />
        <Indicador etiqueta="Respuestas" valor={reporte.totalRespuestasAnonimas} detalle={detalleVersion} />
        <Indicador etiqueta="Periodo analizado" valor={<span className="text-md">{periodo ?? 'Sin respuestas'}</span>} detalle={periodo ? 'Primera y última respuesta' : undefined} />
      </dl>

      <PeligrosSugeridos encuestaId={encuesta.id} />

      {reporte.detalleOculto ? (
        <Aviso tipo="info" titulo="Los resultados por pregunta aparecen con 3 respuestas">
          Para proteger el anonimato, no se muestra el detalle hasta que haya al menos {MINIMO_DETALLE} respuestas en esta versión.
          {esHistorica || encuesta.estado === 'cerrada'
            ? ` Tuvo ${plural(reporte.totalRespuestasAnonimas, 'respuesta', 'respuestas')}.`
            : ` Van ${plural(reporte.totalRespuestasAnonimas, 'respuesta', 'respuestas')}.`}
        </Aviso>
      ) : (
        <section className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-label="Resultados por pregunta">
          {reporte.preguntas.map((p, i) => (
            <ResultadoPregunta key={p.preguntaId} numero={i + 1} pregunta={p} />
          ))}
        </section>
      )}
    </div>
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
