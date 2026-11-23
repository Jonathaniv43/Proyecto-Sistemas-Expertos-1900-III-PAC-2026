import { authClient } from '../../../../shared/api/authClient'
import type { SesionResponseDto } from '../../sesion/types/sesion.types'
import type { LoginEmpresaRequestDto } from '../types/loginEmpresa.types'

export async function postLoginEmpresa(
  datos: LoginEmpresaRequestDto,
): Promise<SesionResponseDto> {
  const { data } = await authClient.post<SesionResponseDto>('/auth/login/empresa', datos)
  return data
}
