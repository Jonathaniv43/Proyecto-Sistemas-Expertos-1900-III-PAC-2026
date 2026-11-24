import { Bell } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { contarNoLeidas } from '../api/notificaciones'
import { alCambiar } from '../cambios'

/** Cada cuánto se vuelve a consultar el contador mientras la pantalla está abierta. */
const INTERVALO_MS = 60_000

/**
 * HU-117: campana del encabezado con el número de notificaciones no leídas; lleva a la bandeja. Se actualiza al
 * cambiar de pantalla, cada minuto mientras la pestaña está visible y al marcar una como leída. Si la consulta falla,
 * la campana queda sin número: no es un error que interrumpa a la persona.
 */
export function CampanaNotificaciones() {
  const { pathname } = useLocation()
  const { datos: noLeidas, ejecutar } = usePeticion(contarNoLeidas)
  const actualizar = useCallback(() => void ejecutar(), [ejecutar])

  useEffect(actualizar, [actualizar, pathname])
  useEffect(() => alCambiar(actualizar), [actualizar])
  useEffect(() => {
    const id = window.setInterval(() => {
      if (document.visibilityState === 'visible') actualizar()
    }, INTERVALO_MS)
    return () => window.clearInterval(id)
  }, [actualizar])

  const cantidad = noLeidas ?? 0
  const etiqueta = cantidad === 0 ? 'Notificaciones' : `Notificaciones, ${cantidad} sin leer`

  return (
    <Link to="/notificaciones" aria-label={etiqueta} title={etiqueta} className="boton-icono relative border border-border-default">
      <Bell aria-hidden size={20} strokeWidth={1.5} />
      {cantidad > 0 && (
        <span
          aria-hidden
          className="absolute -top-1 -right-1 grid h-5 min-w-5 place-items-center rounded-full bg-info-icon px-1 text-xs font-semibold text-bg-surface tabular-nums"
        >
          {cantidad > 9 ? '9+' : cantidad}
        </span>
      )}
    </Link>
  )
}
