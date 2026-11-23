import { sesionStore } from '../../sesion/store/sesionStore'
import { refrescarSesionAsync } from './refrescarSesion'

let refreshEnCurso: Promise<string | null> | null = null

/** Un solo refresh en vuelo: varias 401 concurrentes esperan la misma promesa. */
export function renovarAccessToken(): Promise<string | null> {
  if (!refreshEnCurso) {
    refreshEnCurso = refrescarSesionAsync()
      .then((sesion) => sesion.accessToken)
      .catch(() => {
        sesionStore.limpiar()
        return null
      })
      .finally(() => {
        refreshEnCurso = null
      })
  }
  return refreshEnCurso
}
