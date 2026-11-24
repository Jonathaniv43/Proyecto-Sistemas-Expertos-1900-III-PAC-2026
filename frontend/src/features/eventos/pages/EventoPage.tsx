import { ArrowLeft, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, type ReactNode } from 'react'
import { Link, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerEvento } from '../api/eventos'
import { ESTADOS_ENFERMEDAD, ESTADOS_EVENTO, fechaHora, NOMBRE_TIPO } from '../etiquetas'

/** HU-047: un evento reportado. No muestra quién lo reportó porque no se guarda (RN-27, D11). */
export function EventoPage() {
  const { id = '' } = useParams()
  const peticion = usePeticion(obtenerEvento)
  const cargandoVisible = useCargaVisible(peticion.cargando)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id), [ejecutar, id])
  useEffect(cargar, [cargar])

  if (peticion.error?.status === 404) return <NotFoundPage />
  const e = peticion.datos

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <div>
        <Link to="/eventos" className="inline-flex items-center gap-1 text-sm">
          <ArrowLeft aria-hidden size={16} strokeWidth={1.5} />
          Volver a los eventos
        </Link>
      </div>
      {peticion.error && !e ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar el evento</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !e ? (
        <div aria-busy="true" aria-label="Cargando el evento" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          <header className="grid gap-2">
            <h1 className="text-xl font-semibold">{NOMBRE_TIPO[e.tipo]}</h1>
            <div className="flex flex-wrap items-center gap-2">
              <Insignia tipo={ESTADOS_EVENTO[e.estado].tipo}>{ESTADOS_EVENTO[e.estado].texto}</Insignia>
              {e.esGrave && <Insignia tipo="error">Grave</Insignia>}
            </div>
          </header>
          <section className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
            <dl className="grid gap-x-6 gap-y-4 sm:grid-cols-2">
              <Dato etiqueta="Ocurrió">{fechaHora(e.fechaHoraEvento)}</Dato>
              <Dato etiqueta="Lugar">{e.lugar ?? '—'}</Dato>
              {e.estadoEnfermedad && (
                <Dato etiqueta="Enfermedad">{ESTADOS_ENFERMEDAD.find((x) => x.valor === e.estadoEnfermedad)?.texto}</Dato>
              )}
              <Dato etiqueta="Reporte recibido">{fechaHora(e.registradoEn)}</Dato>
            </dl>
            <div className="grid gap-1">
              <h2 className="text-xs text-text-secondary">Qué ocurrió</h2>
              <p className="wrap-break-word whitespace-pre-line">{e.descripcion}</p>
            </div>
            <p className="text-sm text-text-secondary">El reporte es anónimo: Norvia no guarda quién lo envió.</p>
          </section>
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
