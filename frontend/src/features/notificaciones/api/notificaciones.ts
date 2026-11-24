import { api } from '@/shared/api'
import type { MisNotificaciones } from '../types/notificaciones'

/** Mis notificaciones, de la más reciente a la más antigua, con el número de no leídas (HU-117). */
export const listarNotificaciones = (signal: AbortSignal) =>
  api.get<MisNotificaciones>('/notificaciones', { signal }).then((r) => r.data)

/** Cuántas no he leído: lo consulta la campana del encabezado cada cierto tiempo. */
export const contarNoLeidas = (signal: AbortSignal) =>
  api.get<{ noLeidas: number }>('/notificaciones/no-leidas', { signal }).then((r) => r.data.noLeidas)

/** Marca como leída una notificación propia (RN-79). 404 `notificaciones.no_encontrada` si no es suya o no existe. */
export const marcarLeida = (id: string) => api.patch(`/notificaciones/${id}/lectura`).then(() => undefined)
