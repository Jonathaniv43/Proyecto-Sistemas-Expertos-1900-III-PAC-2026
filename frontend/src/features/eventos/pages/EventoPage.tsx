import { ArrowLeft, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { Link, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { ROLES, useTieneRol } from '@/shared/sesion'
import { obtenerEvento } from '../api/eventos'
import { SeccionCierre } from '../components/SeccionCierre'
import { SeccionInvestigacion } from '../components/SeccionInvestigacion'
import { SeccionMedidas } from '../components/SeccionMedidas'
import { SeccionRiesgos } from '../components/SeccionRiesgos'
import { ESTADOS_ENFERMEDAD, ESTADOS_EVENTO, fechaHora, NOMBRE_TIPO } from '../etiquetas'
import type { EventoDetalle } from '../types/eventos'

/**
 * HU-047: un evento reportado. No muestra quién lo reportó porque no se guarda (RN-27, D11).
 * HU-049: su investigación, cierre o descarte; solo el Responsable SST los registra, el admin consulta.
 * HU-050: sus medidas correctivas (tareas del plan) con su avance.
 * HU-051: los riesgos de la matriz que reveló o hizo reevaluar.
 */
export function EventoPage() {
  const { id = '' } = useParams()
  const peticion = usePeticion(obtenerEvento)
  const cargandoVisible = useCargaVisible(peticion.cargando)
  const puedeGestionar = useTieneRol([ROLES.responsableSst])
  // Lo que devuelven investigar, cerrar y descartar reemplaza lo cargado.
  const [actualizado, setActualizado] = useState<EventoDetalle>()
  const [mensaje, setMensaje] = useState<string>()
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id), [ejecutar, id])
  useEffect(cargar, [cargar])

  const alCambiar = (nuevo: EventoDetalle, texto: string) => {
    setActualizado(nuevo)
    setMensaje(texto)
  }

  if (peticion.error?.status === 404) return <NotFoundPage />
  const e = actualizado?.id === id ? actualizado : peticion.datos

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
          <div role="status" className="contents">
            {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
          </div>
          {e.estado === 'descartado' && (
            <Aviso tipo="info" titulo="Reporte descartado">
              {`Lo descartó ${e.descartadoPorNombre ?? 'el Responsable SST'} el ${fechaHora(e.descartadoEn!)}. ${e.motivoDescarte ? `Motivo: ${e.motivoDescarte}` : 'Sin motivo indicado.'} No cuenta en las estadísticas.`}
            </Aviso>
          )}
          {e.estado === 'cerrado' && e.cerradoEn && (
            <Aviso tipo="info" titulo="Evento cerrado">{`Se cerró el ${fechaHora(e.cerradoEn)}, con su investigación y sus medidas correctivas.`}</Aviso>
          )}
          <SeccionInvestigacion evento={e} puedeGestionar={puedeGestionar} onCambio={alCambiar} />
          <SeccionMedidas evento={e} puedeGestionar={puedeGestionar} onCambio={alCambiar} />
          <SeccionRiesgos evento={e} puedeGestionar={puedeGestionar} />
          {puedeGestionar && (e.estado === 'abierto' || e.estado === 'en_investigacion') && (
            <SeccionCierre evento={e} onCambio={alCambiar} />
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
