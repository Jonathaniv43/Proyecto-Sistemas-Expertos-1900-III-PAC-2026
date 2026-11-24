import { Plus, RefreshCw, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { CuerpoModal, Modal, PieModal } from '@/shared/components/ui/Modal'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { EtiquetaRiesgo } from '@/shared/riesgo/EtiquetaRiesgo'
import { ETIQUETAS_CLASIFICACION_RIESGO } from '@/shared/riesgo/niveles'
import { listarRiesgosDelEvento, listarRiesgosDeMatriz } from '../api/eventos'
import type { EventoDetalle, RiesgoDelEvento } from '../types/eventos'

interface Props {
  evento: EventoDetalle
  /** Responsable SST: agrega o reevalúa riesgos a raíz del evento. El admin solo consulta. */
  puedeGestionar: boolean
}

/**
 * HU-051: lo que el evento cambió en la matriz de riesgos (art. 47.b). Muestra los riesgos que reveló y los que se
 * reevaluaron a raíz de él. El Responsable SST agrega un riesgo nuevo enlazado al evento o elige uno de la matriz para
 * reevaluarlo; las dos cosas se hacen en las pantallas de riesgos, que vuelven al evento. También después de cerrarlo:
 * lo aprendido puede llegar más tarde. Un reporte descartado no se enlaza.
 */
export function SeccionRiesgos({ evento, puedeGestionar }: Props) {
  const peticion = usePeticion(listarRiesgosDelEvento)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const [eligiendo, setEligiendo] = useState(false)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(evento.id), [ejecutar, evento.id])
  useEffect(cargar, [cargar])

  if (evento.estado === 'descartado') return null
  const riesgos = peticion.datos

  return (
    <section aria-labelledby="titulo-riesgos" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="grid gap-1">
        <h2 id="titulo-riesgos" className="text-md font-semibold">
          Matriz de riesgos
        </h2>
        <p className="text-text-secondary">Lo aprendido del evento: los riesgos que reveló y los que se reevaluaron por él.</p>
      </header>

      {peticion.error && !riesgos ? (
        <div className="flex flex-col items-center gap-3 py-6 text-center">
          <TriangleAlert aria-hidden size={24} strokeWidth={1.5} className="text-error-icon" />
          <p className="text-text-secondary">{`No se pudieron cargar los riesgos. ${peticion.error.mensaje}`}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </div>
      ) : !riesgos ? (
        <div aria-busy="true" aria-label="Cargando los riesgos" className="grid gap-2">
          {cargandoVisible &&
            [0, 1].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : riesgos.length === 0 ? (
        <p className="text-text-secondary">Todavía no se enlaza con ningún riesgo de la matriz.</p>
      ) : (
        <ul className="grid divide-y divide-border-default border-y border-border-default">
          {riesgos.map((r) => (
            <Riesgo key={r.riesgoId} riesgo={r} />
          ))}
        </ul>
      )}

      {puedeGestionar && (
        <div className="flex flex-col items-stretch gap-3 sm:flex-row sm:items-center">
          <Link to={`/riesgos/nuevo?evento=${evento.id}`} className={clasesBoton('secundario')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Agregar un riesgo
          </Link>
          <Boton variante="secundario" onClick={() => setEligiendo(true)}>
            <RefreshCw aria-hidden size={16} strokeWidth={1.5} />
            Reevaluar un riesgo
          </Boton>
        </div>
      )}

      <Modal abierto={eligiendo} titulo="Reevaluar un riesgo de la matriz" onCerrar={() => setEligiendo(false)}>
        {eligiendo && <ElegirRiesgo eventoId={evento.id} onCerrar={() => setEligiendo(false)} />}
      </Modal>
    </section>
  )
}

const nivelTexto = (nivel: number, clasificacion: RiesgoDelEvento['clasificacion']) =>
  `${ETIQUETAS_CLASIFICACION_RIESGO[clasificacion!]} · ${nivel}`

function Riesgo({ riesgo: r }: { riesgo: RiesgoDelEvento }) {
  const nivel = r.nivel !== null && r.clasificacion ? { nivel: r.nivel, clasificacion: r.clasificacion } : null
  const relacion =
    r.relacion === 'revelado'
      ? 'Lo reveló este evento.'
      : r.nivelAnterior !== null
        ? `Se reevaluó por este evento: pasó de ${nivelTexto(r.nivelAnterior, r.clasificacionAnterior)} a ${nivelTexto(r.nivel!, r.clasificacion)}.`
        : 'Se evaluó por primera vez a raíz de este evento.'
  return (
    <li className="grid gap-1 py-3">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <Link to={`/riesgos/${r.riesgoId}`} className="wrap-break-word font-medium">
          {r.nombre}
        </Link>
        <EtiquetaRiesgo nivel={nivel} />
      </div>
      {r.actividad && <p className="text-xs text-text-secondary">{r.actividad}</p>}
      <p className="text-sm text-text-secondary">{relacion}</p>
    </li>
  )
}

/** Elige un riesgo de la matriz y lleva a reevaluarlo en su página, enlazado con el evento. */
function ElegirRiesgo({ eventoId, onCerrar }: { eventoId: string; onCerrar: () => void }) {
  const navigate = useNavigate()
  const matriz = usePeticion(listarRiesgosDeMatriz)
  const { ejecutar } = matriz
  useEffect(() => {
    void ejecutar()
  }, [ejecutar])
  const [riesgoId, setRiesgoId] = useState('')
  const [intento, setIntento] = useState(false)

  const opciones = (matriz.datos ?? []).map((r) => ({
    valor: r.id,
    texto: r.peligroNombre ?? r.descripcionPeligro ?? 'Riesgo',
    descripcion: [r.actividad, r.evaluacion ? nivelTexto(r.evaluacion.nivel, r.evaluacion.clasificacion) : 'Sin evaluar']
      .filter(Boolean)
      .join(' · '),
  }))

  return (
    <form
      noValidate
      onSubmit={(e) => {
        e.preventDefault()
        setIntento(true)
        if (riesgoId) navigate(`/riesgos/${riesgoId}?evento=${eventoId}`)
      }}
    >
      <CuerpoModal>
        <p className="text-text-secondary">
          Elige el riesgo que el evento hace revisar. En su página lo evalúas de nuevo y la evaluación queda enlazada con el
          evento; la anterior se conserva en su historial.
        </p>
        {matriz.error && <Aviso tipo="error">{`No se pudo cargar la matriz. ${matriz.error.mensaje}`}</Aviso>}
        {matriz.datos?.length === 0 ? (
          <p>La matriz todavía no tiene riesgos. Agrega uno nuevo desde el evento.</p>
        ) : (
          <SelectBusqueda
            etiqueta="Riesgo"
            required
            etiquetaBusqueda="Buscar riesgo"
            opciones={opciones}
            valor={riesgoId}
            onChange={setRiesgoId}
            error={intento && !riesgoId ? 'Elige el riesgo que vas a reevaluar.' : undefined}
          />
        )}
      </CuerpoModal>
      <PieModal>
        <Boton variante="secundario" onClick={onCerrar}>
          Cancelar
        </Boton>
        <Boton type="submit" disabled={matriz.datos?.length === 0}>
          Reevaluar
        </Boton>
      </PieModal>
    </form>
  )
}
