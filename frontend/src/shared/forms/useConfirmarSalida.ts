import { useEffect } from 'react'
import { useBlocker, type Location } from 'react-router-dom'

/**
 * Pide confirmar antes de salir de un formulario con datos sin enviar (design-system.md § Diálogo de confirmación):
 * - dentro de Norvia (menú, "Volver", campana, "Cancelar"): bloquea la navegación y la pantalla muestra un
 *   DialogoConfirmacion con `bloqueado`, `salir` y `quedarse`;
 * - al recargar o cerrar la pestaña: avisa el navegador.
 *
 * `permitir` deja pasar sin preguntar una salida esperada (p. ej. la de "Guardar").
 */
export function useConfirmarSalida(activo: boolean, permitir?: (destino: Location) => boolean) {
  useEffect(() => {
    if (!activo) return
    const avisar = (e: BeforeUnloadEvent) => e.preventDefault()
    window.addEventListener('beforeunload', avisar)
    return () => window.removeEventListener('beforeunload', avisar)
  }, [activo])

  const bloqueo = useBlocker(
    ({ currentLocation, nextLocation }) =>
      activo && currentLocation.pathname !== nextLocation.pathname && !(permitir?.(nextLocation) ?? false),
  )

  return {
    bloqueado: bloqueo.state === 'blocked',
    salir: () => bloqueo.proceed?.(),
    quedarse: () => bloqueo.reset?.(),
  }
}
