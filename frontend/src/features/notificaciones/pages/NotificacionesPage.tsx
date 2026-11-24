import { Bell, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { extraerError } from '@/shared/api/errores'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { useScrollRecordado } from '@/shared/hooks/useScrollRecordado'
import { listarNotificaciones, marcarLeida } from '../api/notificaciones'
import { avisarCambio, destinoDe } from '../cambios'
import type { Notificacion } from '../types/notificaciones'

const fechaHora = (iso: string) =>
  new Date(iso).toLocaleString('es-HN', { day: 'numeric', month: 'short', year: 'numeric', hour: 'numeric', minute: '2-digit' })

/**
 * HU-117: la bandeja de quien tiene la sesión, de la más reciente a la más antigua, con el número de no leídas. Desde
 * cada notificación se abre el elemento relacionado (al abrirla queda leída) o se marca como leída. Solo muestra las
 * propias y de la empresa de la sesión (RN-79; lo garantiza la API).
 */
export function NotificacionesPage() {
  const peticion = usePeticion(listarNotificaciones)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const [marcando, setMarcando] = useState<string>()
  const [error, setError] = useState<string>()
  const navigate = useNavigate()
  useScrollRecordado(Boolean(peticion.datos))

  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(), [ejecutar])
  useEffect(cargar, [cargar])

  const marcar = async (n: Notificacion) => {
    setMarcando(n.id)
    setError(undefined)
    try {
      await marcarLeida(n.id)
      avisarCambio()
      await ejecutar()
      return true
    } catch (causa) {
      setError(`No se pudo marcar como leída: ${extraerError(causa).mensaje}`)
      return false
    } finally {
      setMarcando(undefined)
    }
  }

  const abrir = async (n: Notificacion, destino: string) => {
    // Abrir el elemento relacionado cuenta como leerla; si no se pudo marcar, igual se abre.
    if (!n.leidaEn) await marcar(n)
    void navigate(destino)
  }

  const datos = peticion.datos
  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/inicio" texto="Volver al inicio" titulo="Notificaciones" />
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Notificaciones</h1>
        <p className="text-text-secondary">
          {datos && datos.noLeidas > 0
            ? `Tienes ${datos.noLeidas} ${datos.noLeidas === 1 ? 'notificación sin leer' : 'notificaciones sin leer'}.`
            : 'Las novedades de tu empresa: tareas que te asignan y políticas nuevas.'}
        </p>
      </header>

      {error && <Aviso tipo="error">{error}</Aviso>}

      {peticion.error && !datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar tus notificaciones</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !datos ? (
        <div aria-busy="true" aria-label="Cargando tus notificaciones" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : datos.notificaciones.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <Bell aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">No tienes notificaciones</h2>
          <p className="text-text-secondary">Cuando te asignen una tarea o se publique una política, aparecerá aquí.</p>
        </section>
      ) : (
        <ul aria-label="Notificaciones" className="grid gap-3">
          {datos.notificaciones.map((n) => (
            <ItemNotificacion
              key={n.id}
              notificacion={n}
              cargando={marcando === n.id}
              ocupada={marcando !== undefined}
              onMarcar={() => void marcar(n)}
              onAbrir={(destino) => void abrir(n, destino)}
            />
          ))}
        </ul>
      )}
    </div>
  )
}

interface ItemNotificacionProps {
  notificacion: Notificacion
  cargando: boolean
  ocupada: boolean
  onMarcar: () => void
  onAbrir: (destino: string) => void
}

function ItemNotificacion({ notificacion: n, cargando, ocupada, onMarcar, onAbrir }: ItemNotificacionProps) {
  const destino = destinoDe(n)
  const noLeida = !n.leidaEn

  return (
    <li className={`grid gap-3 rounded-lg border p-4 ${noLeida ? 'border-info-border bg-bg-surface' : 'border-border-default bg-bg-canvas'}`}>
      <div className="grid gap-1">
        <div className="flex items-start gap-2">
          {noLeida && <span aria-hidden className="mt-2 size-2 shrink-0 rounded-full bg-info-icon" />}
          <h2 className={noLeida ? 'font-semibold' : 'font-medium text-text-secondary'}>
            {noLeida && <span className="sr-only">Sin leer: </span>}
            {n.titulo}
          </h2>
        </div>
        <p className="wrap-break-word">{n.mensaje}</p>
        <p className="text-xs text-text-secondary">
          <time dateTime={n.creadaEn}>{fechaHora(n.creadaEn)}</time>
        </p>
      </div>

      {(destino || noLeida) && (
        <div className="flex flex-wrap items-center gap-2">
          {destino && (
            <Link
              to={destino}
              className="inline-flex min-h-control-touch items-center text-sm font-medium sm:min-h-control"
              onClick={(e) => {
                if (e.metaKey || e.ctrlKey || e.shiftKey || e.button !== 0) return
                e.preventDefault()
                onAbrir(destino)
              }}
            >
              {n.tipo === 'tarea_asignada' ? 'Ver mis tareas' : 'Abrir'}
            </Link>
          )}
          {noLeida && (
            <Boton variante="secundario" cargando={cargando} textoCargando="Marcando…" disabled={ocupada && !cargando} onClick={onMarcar}>
              Marcar como leída
            </Boton>
          )}
        </div>
      )}
    </li>
  )
}
