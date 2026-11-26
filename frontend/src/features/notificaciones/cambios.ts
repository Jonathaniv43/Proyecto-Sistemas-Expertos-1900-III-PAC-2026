import type { Notificacion } from './types/notificaciones'

const EVENTO = 'norvia:notificaciones'

/** Avisa a la campana que cambió el número de no leídas (se marcó una como leída). */
export const avisarCambio = () => window.dispatchEvent(new Event(EVENTO))

/** La campana se suscribe para actualizar el contador sin esperar al próximo sondeo. */
export function alCambiar(callback: () => void) {
  window.addEventListener(EVENTO, callback)
  return () => window.removeEventListener(EVENTO, callback)
}

/**
 * Pantalla del elemento relacionado (HU-117): la tarea asignada se atiende en Mis tareas; la política publicada abre
 * su versión vigente (HU-116, HU-074). Si después se aprobó otra versión, se ve la más nueva.
 */
export const destinoDe = (n: Notificacion): string | undefined => {
  switch (n.tipo) {
    case 'tarea_asignada':
      return '/mis-tareas'
    case 'politica_publicada':
      return n.documentoId ? `/documentos-vigentes/${n.documentoId}` : undefined
  }
}
