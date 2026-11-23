import { sesionStore } from '../../sesion/store/sesionStore'
import { postRefrescarSesion } from '../api/refrescarSesionApi'
import type { SesionResponseDto } from '../../sesion/types/sesion.types'

export async function refrescarSesionAsync(): Promise<SesionResponseDto> {
  const sesion = await postRefrescarSesion()
  sesionStore.aplicarSesion(sesion)
  return sesion
}
