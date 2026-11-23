import { authClient } from '../../../../shared/api/authClient'
import type { SesionResponseDto } from '../../sesion/types/sesion.types'
import type { LoginPlataformaRequestDto } from '../types/loginPlataforma.types'

export async function postLoginPlataforma(
  datos: LoginPlataformaRequestDto,
): Promise<SesionResponseDto> {
  const { data } = await authClient.post<SesionResponseDto>('/auth/login/plataforma', datos)
  return data
}
