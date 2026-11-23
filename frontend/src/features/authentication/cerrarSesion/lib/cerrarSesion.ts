import { sesionStore } from '../../sesion/store/sesionStore'
import { postCerrarSesion } from '../api/cerrarSesionApi'

export async function cerrarSesionAsync(): Promise<void> {
  try {
    await postCerrarSesion()
  } finally {
    sesionStore.limpiar()
  }
}
