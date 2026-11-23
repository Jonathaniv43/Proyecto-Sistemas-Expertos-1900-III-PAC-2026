import { authClient } from '../../../../shared/api/authClient'
import type { SesionResponseDto } from '../../sesion/types/sesion.types'

/** El refresh puede ir solo en cookie HttpOnly (body vacío). */
export async function postRefrescarSesion(): Promise<SesionResponseDto> {
  const { data } = await authClient.post<SesionResponseDto>('/auth/refresh', {})
  return data
}
