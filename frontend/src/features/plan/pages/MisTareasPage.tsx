import { ListChecks, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { extraerError } from '@/shared/api/errores'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { fechaCorta } from '@/shared/validacion'
import { cambiarEstadoTarea, listarMisTareas } from '../api/plan'
import { ESTADOS, PRIORIDADES, TIPOS_CONTROL, vencida } from '../etiquetas'
import type { EstadoTarea, Tarea } from '../types/plan'

type EstadoAtendible = Exclude<EstadoTarea, 'cancelada'>

const QUEDO: Record<EstadoAtendible, string> = {
  pendiente: 'quedó pendiente',
  en_progreso: 'quedó en progreso',
  completada: 'quedó completada',
}

const abierta = (t: Tarea) => t.estado === 'pendiente' || t.estado === 'en_progreso'

/**
 * HU-044: las tareas a cargo de quien tiene la sesión. Las pasa por pendiente, en progreso y completada (y puede
 * reabrirlas); solo ve las suyas, así que no puede cambiar tareas ajenas (RN-24). Si una tarea pide evidencia, no se
 * completa sin ella (RN-25): adjuntarla es HU-045.
 */
export function MisTareasPage() {
  const peticion = usePeticion(listarMisTareas)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const [mensaje, setMensaje] = useState<string>()
  const [cambiando, setCambiando] = useState<string>()
  const [errores, setErrores] = useState<Record<string, string>>({})
  // La tarea ya no está en la lista (se reasignó o se borró): el error no puede ir junto a ella.
  const [errorGeneral, setErrorGeneral] = useState<string>()

  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(), [ejecutar])
  useEffect(cargar, [cargar])

  const cambiar = async (tarea: Tarea, estado: EstadoAtendible) => {
    setCambiando(tarea.id)
    setMensaje(undefined)
    setErrorGeneral(undefined)
    setErrores((e) => Object.fromEntries(Object.entries(e).filter(([id]) => id !== tarea.id)))
    try {
      const actualizada = await cambiarEstadoTarea(tarea.id, estado)
      setMensaje(`«${actualizada.titulo}» ${QUEDO[estado]}.`)
      await ejecutar()
    } catch (causa) {
      const error = extraerError(causa)
      if (error.status === 403 || error.status === 404) {
        // Ya no es suya: se actualiza la lista y el aviso queda arriba.
        setErrorGeneral(`No se pudo cambiar «${tarea.titulo}»: ${error.status === 403 ? 'ya no está a tu cargo.' : 'ya no existe.'}`)
        await ejecutar()
      } else setErrores((e) => ({ ...e, [tarea.id]: error.mensaje }))
    } finally {
      setCambiando(undefined)
    }
  }

  const tareas = peticion.datos ?? []
  const porHacer = tareas.filter(abierta)
  const terminadas = tareas.filter((t) => !abierta(t))

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Mis tareas</h1>
        <p className="text-text-secondary">Las tareas que tienes a cargo. Marca tu avance para que el plan esté al día.</p>
      </header>

      <div role="status" className="contents">
        {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
      </div>
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

      {peticion.error && !peticion.datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar tus tareas</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !peticion.datos ? (
        <div aria-busy="true" aria-label="Cargando tus tareas" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : tareas.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ListChecks aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">No tienes tareas asignadas</h2>
          <p className="text-text-secondary">Cuando te asignen una tarea del plan de trabajo, aparecerá aquí.</p>
        </section>
      ) : (
        <>
          <Grupo titulo="Por hacer" vacio="No tienes tareas pendientes." tareas={porHacer} cambiando={cambiando} errores={errores} onCambiar={cambiar} />
          {terminadas.length > 0 && (
            <Grupo titulo="Terminadas" tareas={terminadas} cambiando={cambiando} errores={errores} onCambiar={cambiar} />
          )}
        </>
      )}
    </div>
  )
}

interface GrupoProps {
  titulo: string
  vacio?: string
  tareas: Tarea[]
  cambiando: string | undefined
  errores: Record<string, string>
  onCambiar: (tarea: Tarea, estado: EstadoAtendible) => void
}

function Grupo({ titulo, vacio, tareas, cambiando, errores, onCambiar }: GrupoProps) {
  const id = `grupo-${titulo.toLowerCase().replace(/\s/g, '-')}`
  return (
    <section aria-labelledby={id} className="grid gap-3">
      <h2 id={id} className="text-md font-semibold">
        {titulo} <span className="font-normal text-text-secondary">({tareas.length})</span>
      </h2>
      {tareas.length === 0 ? (
        <p className="text-text-secondary">{vacio}</p>
      ) : (
        <ul aria-label={titulo} className="grid gap-3">
          {tareas.map((t) => (
            <ItemTarea key={t.id} tarea={t} ocupada={cambiando !== undefined} cargando={cambiando === t.id} error={errores[t.id]} onCambiar={onCambiar} />
          ))}
        </ul>
      )}
    </section>
  )
}

interface ItemTareaProps {
  tarea: Tarea
  ocupada: boolean
  cargando: boolean
  error: string | undefined
  onCambiar: (tarea: Tarea, estado: EstadoAtendible) => void
}

function ItemTarea({ tarea, ocupada, cargando, error, onCambiar }: ItemTareaProps) {
  const estado = ESTADOS[tarea.estado]
  const origen =
    tarea.origen === 'riesgo'
      ? `Riesgo: ${tarea.riesgoNombre ?? 'sin nombre'}${tarea.controlTipo ? ` · Control: ${TIPOS_CONTROL[tarea.controlTipo].toLowerCase()}` : ''}`
      : tarea.origen === 'accidente'
        ? 'Medida de un accidente'
        : 'Tarea manual'
  const faltaEvidencia = tarea.requiereEvidencia

  return (
    <li className="grid gap-3 rounded-lg border border-border-default bg-bg-surface p-4">
      <div className="grid gap-1">
        <div className="flex flex-wrap items-start justify-between gap-2">
          <h3 className="font-medium">{tarea.titulo}</h3>
          <span className="inline-flex flex-wrap gap-1">
            <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
            {vencida(tarea) && <Insignia tipo="error">Vencida</Insignia>}
          </span>
        </div>
        <p className="text-xs text-text-secondary">
          {[
            `Prioridad ${PRIORIDADES[tarea.prioridad].toLowerCase()}`,
            tarea.fechaLimite && `Hasta el ${fechaCorta(tarea.fechaLimite)}`,
            tarea.completadaEn && `Completada el ${new Date(tarea.completadaEn).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })}`,
          ]
            .filter(Boolean)
            .join(' · ')}
        </p>
        <p className="text-xs text-text-secondary">{origen}</p>
      </div>
      {tarea.descripcion && tarea.descripcion !== tarea.titulo && (
        <p className="wrap-break-word whitespace-pre-line">{tarea.descripcion}</p>
      )}
      {faltaEvidencia && tarea.estado !== 'completada' && tarea.estado !== 'cancelada' && (
        <p className="text-sm text-text-secondary">Pide evidencia: para completarla tendrás que adjuntar una foto o un PDF.</p>
      )}
      {error && <Aviso tipo="error">{error}</Aviso>}

      {tarea.estado !== 'cancelada' && (
        <div className="flex flex-wrap items-center gap-2">
          {tarea.estado === 'pendiente' && (
            <Boton variante="secundario" cargando={cargando} textoCargando="Guardando…" disabled={ocupada && !cargando} onClick={() => onCambiar(tarea, 'en_progreso')}>
              Empezar
            </Boton>
          )}
          {tarea.estado === 'en_progreso' && (
            <>
              <Boton
                variante="secundario"
                cargando={cargando}
                textoCargando="Guardando…"
                disabled={(ocupada && !cargando) || faltaEvidencia}
                onClick={() => onCambiar(tarea, 'completada')}
              >
                Marcar como completada
              </Boton>
              <BotonEnlace disabled={ocupada} onClick={() => onCambiar(tarea, 'pendiente')}>
                Volver a pendiente
              </BotonEnlace>
            </>
          )}
          {tarea.estado === 'completada' && (
            <BotonEnlace disabled={ocupada} onClick={() => onCambiar(tarea, 'en_progreso')}>
              Reabrir
            </BotonEnlace>
          )}
        </div>
      )}
    </li>
  )
}

function BotonEnlace({ disabled, onClick, children }: { disabled: boolean; onClick: () => void; children: string }) {
  return (
    <button
      type="button"
      disabled={disabled}
      className="inline-flex min-h-control-touch items-center px-2 text-sm font-medium text-text-link hover:text-text-link-hover disabled:text-text-disabled sm:min-h-control"
      onClick={onClick}
    >
      {children}
    </button>
  )
}
