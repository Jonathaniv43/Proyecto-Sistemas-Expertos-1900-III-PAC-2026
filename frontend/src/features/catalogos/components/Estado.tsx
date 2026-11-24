import { Insignia } from '@/shared/components/ui/Insignia'

/** Activo (éxito) o inactivo (neutro: fuera de uso), como define design-system.md § Badges de estado. */
export function EstadoActivo({ activo }: { activo: boolean }) {
  return <Insignia tipo={activo ? 'exito' : 'neutro'}>{activo ? 'Activo' : 'Inactivo'}</Insignia>
}

interface BotonEstadoProps {
  activo: boolean
  nombre: string
  ocupado: boolean
  onCambiar: (activo: boolean) => void
}

/** Desactivar o reactivar: no se borra nada, así que no pide confirmación. */
export function BotonEstado({ activo, nombre, ocupado, onCambiar }: BotonEstadoProps) {
  return (
    <button
      type="button"
      disabled={ocupado}
      aria-label={`${activo ? 'Desactivar' : 'Activar'} «${nombre}»`}
      className="inline-flex min-h-control-touch items-center px-2 text-sm font-medium text-text-link hover:text-text-link-hover disabled:text-text-disabled sm:min-h-control"
      onClick={() => onCambiar(!activo)}
    >
      {activo ? 'Desactivar' : 'Activar'}
    </button>
  )
}
