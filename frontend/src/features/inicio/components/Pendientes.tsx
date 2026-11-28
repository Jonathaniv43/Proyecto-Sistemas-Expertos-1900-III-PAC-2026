import { ChevronRight, CircleAlert, CircleCheck, type LucideIcon } from 'lucide-react'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { clasesBoton } from '@/shared/components/ui/clases'
import type { EncuestaPorResponder } from '../api/resumen'
import { ordenar, type Fila } from '../filas'

/**
 * Piezas del inicio (design-system.md § Inicio): la lista de pendientes, sus filas, la encuesta destacada y los
 * estados (cargando, al día).
 */

interface FilaPendienteProps {
  a: string
  Icono: LucideIcon
  nombre: string
  detalle: string
  insignia?: { texto: string; tipo: 'error' | 'advertencia' }
}

/** Una cosa por atender: el módulo, la cifra y su detalle. Toda la fila es el link a donde se atiende. */
export function FilaPendiente({ a, Icono, nombre, detalle, insignia }: FilaPendienteProps) {
  return (
    <Link to={a} className="fila-pendiente">
      <Icono aria-hidden size={20} strokeWidth={1.5} className="text-text-secondary" />
      <span className="grid min-w-0">
        <span className="font-medium">{nombre}</span>
        <span className="text-text-secondary">{detalle}</span>
      </span>
      {insignia ? <Insignia tipo={insignia.tipo}>{insignia.texto}</Insignia> : <span />}
      <ChevronRight aria-hidden size={16} strokeWidth={1.5} className="fila-pendiente-flecha text-text-tertiary" />
    </Link>
  )
}

/** Una fuente que no cargó: en su lugar, con "Reintentar" (design-system.md § Estados de pantalla). */
export function FilaError({ mensaje, onReintentar, reintentando }: { mensaje: string; onReintentar: () => void; reintentando: boolean }) {
  return (
    <div className="fila-pendiente fila-pendiente-error" role="alert">
      <CircleAlert aria-hidden size={20} strokeWidth={1.5} className="text-error-icon" />
      <span className="grid min-w-0">
        <span className="font-medium">{mensaje}</span>
        <span className="text-text-secondary">Revisa tu conexión e inténtalo de nuevo.</span>
      </span>
      <Boton variante="secundario" cargando={reintentando} textoCargando="Reintentando…" onClick={onReintentar}>
        Reintentar
      </Boton>
    </div>
  )
}

/** Lista de filas en una sola card; con `titulo`, como grupo ("Para ti", "En la empresa"). */
export function ListaPendientes({ titulo, filas }: { titulo?: string; filas: Fila[] }) {
  if (filas.length === 0) return null
  const lista = (
    <ul className="lista-pendientes">
      {ordenar(filas).map((f) => (
        <li key={f.clave}>{f.nodo}</li>
      ))}
    </ul>
  )
  if (!titulo) return lista
  return (
    <section className="grid gap-2" aria-label={titulo}>
      <h2 className="text-md font-semibold">{titulo}</h2>
      {lista}
    </section>
  )
}

/**
 * La encuesta por responder, destacada con el único botón principal del inicio: con una sola, abre directo esa
 * encuesta; con varias, la lista de Mis encuestas.
 */
export function EncuestaDestacada({ encuestas }: { encuestas: EncuestaPorResponder[] }) {
  if (encuestas.length === 0) return null
  const una = encuestas.length === 1
  return (
    <section className="grid gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-labelledby="encuesta-destacada">
      <span className="text-xs text-text-secondary">
        {una ? 'Tienes una encuesta por responder' : `Tienes ${encuestas.length} encuestas por responder`}
      </span>
      <h2 id="encuesta-destacada" className="text-md font-semibold">
        {una ? encuestas[0].titulo : encuestas.map((e) => e.titulo).join(' · ')}
      </h2>
      <p className="text-text-secondary">Tus respuestas son anónimas: nadie sabe qué respondiste.</p>
      <div className="flex flex-col sm:flex-row">
        <Link to={una ? `/mis-encuestas/${encuestas[0].encuestaId}` : '/mis-encuestas'} className={clasesBoton('primario')}>
          {una ? 'Responder ahora' : 'Ver mis encuestas'}
        </Link>
      </div>
    </section>
  )
}

/** Todo cargó y no hay nada por atender. */
export function AlDia() {
  return (
    <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
      <CircleCheck aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
      <h2 className="text-md font-semibold">Estás al día</h2>
      <p className="text-text-secondary">Cuando haya algo por atender, aparecerá aquí.</p>
    </section>
  )
}

/** Skeleton con la forma de las filas (solo si la carga pasa de ~300ms). */
export function CargandoPendientes() {
  return (
    <div aria-busy="true" aria-label="Cargando" className="grid gap-2">
      {[0, 1, 2].map((i) => (
        <div key={i} className="h-16 animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
      ))}
    </div>
  )
}
